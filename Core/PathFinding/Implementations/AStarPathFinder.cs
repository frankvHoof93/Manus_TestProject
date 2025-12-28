using dev.vanHoof.ManusTest.Core.Data;
using dev.vanHoof.ManusTest.Core.Pathfinding.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Path = dev.vanHoof.ManusTest.Core.Pathfinding.Data.Path;

namespace dev.vanHoof.ManusTest.Core.Pathfinding.Implementations
{
    /// <summary>
    /// <see cref="IPathFinder"/> that implements A* with Landmark heuristic (ALT).
    /// <para>
    /// https://en.wikipedia.org/wiki/A*_search_algorithm
    /// </para>
    /// </summary>
    public class AStarPathfinder : IPathFinder
    {
        /// <summary>
        /// Factor used to determine number of Landmarks.
        /// <para>
        /// NumCities / Factor = NumLandmarks
        /// </para>
        /// </summary>
        private const int LANDMARK_FACTOR = 10;

        /// <summary>
        /// Number of landmark-distances to (pre-)compute before yielding.
        /// </summary>
        private const int INIT_YIELD_COUNT = 10;

        /// <summary>
        /// A*-Algorithm.
        /// </summary>
        public PathfindingAlgorithm Type => PathfindingAlgorithm.AStar;

        /// <inheritdoc/>
        public bool Initialized { get; private set; }

        private List<City> allCities;
        private List<City> landmarks;
        private Dictionary<City, Dictionary<City, uint>> landmarkDistances;

        /// <summary>
        /// Computes Landmark-Distances.
        /// </summary>
        public async Task Initialize(IEnumerable<City> allCities, CancellationToken cancellationToken = default)
        {
            if (allCities == null)
                throw new ArgumentNullException(nameof(allCities), "World cannot be empty");

            this.allCities = allCities.ToList();
            landmarkDistances = new Dictionary<City, Dictionary<City, uint>>();

            int totalCities = this.allCities.Count;
            int landmarkCount = Math.Max(1, totalCities / LANDMARK_FACTOR);

            // Order Landmarks by number of connected streets, then take landmarkCount.
            landmarks = this.allCities
                .OrderByDescending(c => c.Streets.Count)
                .Take(landmarkCount)
                .ToList();

            int numCompleted = 0;

            // Compute shortest paths from each landmark
            foreach (City landmark in landmarks)
            {
                landmarkDistances[landmark] = SingleSourceShortestPaths(
                        landmark,
                        out _,
                        (a, b) => 0 // Empty heuristic.
                    );
                numCompleted += landmarkDistances[landmark].Count;

                if (INIT_YIELD_COUNT > 0 && numCompleted / INIT_YIELD_COUNT > 0)
                {
                    await Task.Yield(); // keep async-friendly
                    cancellationToken.ThrowIfCancellationRequested();
                    // Keep remainder for next cycle.
                    numCompleted %= INIT_YIELD_COUNT;
                }
            }

            Initialized = true;
        }

        /// <inheritdoc/>
        public Path FindPathBetween(City start, City end)
        {
            if (!Initialized)
                throw new InvalidOperationException("PathFinder has not been initialized.");

            if (start == null)
                throw new ArgumentNullException(nameof(start), "Starting City cannot be NULL");

            if (end == null)
                throw new ArgumentNullException(nameof(end), "Ending City cannot be NULL");

            if (start == end)
                return new Path(new City[] { start });

            // Compute shortest path with ALT heuristic.
            Dictionary<City, uint> distances = SingleSourceShortestPaths(start, out Dictionary<City, City> cameFrom, LandmarkHeuristic, end);

            if (!cameFrom.ContainsKey(end))
                throw new InvalidOperationException($"Could not create path between {start.Name} and {end.Name}");

            // Traverse End->Start for Path
            List<City> finalPath = new List<City>();
            City current = end;
            finalPath.Add(current);
            while (current != start)
            {
                current = cameFrom[current];
                finalPath.Add(current);
            }
            // End->Start is reversed to Start->End
            finalPath.Reverse();

            // Return Path
            return new Path(finalPath.ToArray());
        }

        /// <summary>
        /// Landmark-based Heuristics for distance.
        /// </summary>
        /// <param name="a">City A</param>
        /// <param name="b">City B</param>
        /// <returns>Landmark-based max Distance between Cities.</returns>
        private uint LandmarkHeuristic(City a, City b)
        {
            if (a == b) return 0;
            if (a == null || b == null) return 0;

            uint maxEstimate = 0;
            foreach (City landmark in landmarks)
            {
                uint distToA = landmarkDistances[landmark][a];
                uint distToB = landmarkDistances[landmark][b];
                uint estimate = (uint)Math.Abs((long)distToB - distToA);
                if (estimate > maxEstimate)
                    maxEstimate = estimate;
            }
            return maxEstimate;
        }

        /// <summary>
        /// Single-source shortest-path using Dijkstra + heuristic.
        /// Early-out if 'target' is provided.
        /// </summary>
        /// <param name="source">Source for Paths.</param>
        /// <param name="cameFrom">OUT: Connecting Dictionary</param>
        /// <param name="heuristic">Heuristic to use during distance-calc</param>
        /// <param name="target">Target-City (default NULL)</param>
        /// <returns>Distances for connected Cities.</returns>
        private Dictionary<City, uint> SingleSourceShortestPaths(City source, out Dictionary<City, City> cameFrom, Func<City, City, uint> heuristic, City target = null)
        {
            // Init vars.
            cameFrom = new Dictionary<City, City>();
            Dictionary<City, uint> gScore = new Dictionary<City, uint>();
            SortedSet<CityScore> openSet = new SortedSet<CityScore>();

            // Add Source
            gScore[source] = 0;
            openSet.Add(new CityScore(source, heuristic(source, target ?? source)));

            while (openSet.Count > 0)
            {
                CityScore currentScore = openSet.Min;
                openSet.Remove(currentScore);

                City current = currentScore.City;
                uint currG = gScore[current];

                if (target != null && current == target)
                    break;

                foreach (Street street in current.Streets)
                {
                    City neighbor = street.GetOtherCity(current);
                    uint tentativeG = currG + street.Length;

                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        // remove old score if exists
                        if (gScore.ContainsKey(neighbor))
                        {
                            uint oldF = gScore[neighbor] + heuristic(neighbor, target ?? neighbor);
                            openSet.Remove(new CityScore(neighbor, oldF));
                        }

                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;

                        uint fScore = tentativeG + heuristic(neighbor, target ?? neighbor);
                        openSet.Add(new CityScore(neighbor, fScore));
                    }
                }
            }

            return gScore;
        }
    }
}