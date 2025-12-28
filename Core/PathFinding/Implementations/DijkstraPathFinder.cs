using dev.vanHoof.ManusTest.Core.Data;
using dev.vanHoof.ManusTest.Core.Pathfinding.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Path = dev.vanHoof.ManusTest.Core.Pathfinding.Data.Path;

namespace dev.vanHoof.ManusTest.Core.Pathfinding.Implementations
{
    /// <summary>
    /// <see cref="IPathFinder"/> that implements the Dijkstra Algorithm.
    /// <para>
    /// https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
    /// </para>
    /// </summary>
    public class DijkstraPathFinder : IPathFinder
    {
        /// <summary>
        /// Dijkstra's Algorithm.
        /// </summary>
        public PathfindingAlgorithm Type => PathfindingAlgorithm.Dijkstra;

        /// <summary>
        /// Always returns True. Dijkstra does not require Initialization.
        /// </summary>
        public bool Initialized => true;

        /// <summary>
        /// No Implementation. Dijkstra does not require Initialization.
        /// </summary>
        public Task Initialize(IEnumerable<City> allCities, CancellationToken cancellationToken)
            => Task.CompletedTask;

        /// <inheritdoc/>
        public Path FindPathBetween(City start, City end)
        {
            if (start == null)
                throw new ArgumentNullException(nameof(start), "Starting City cannot be NULL");

            if (end == null)
                throw new ArgumentNullException(nameof(end), "Ending City cannot be NULL");

            if (start == end)
                return new Path(new City[] { start });

            Dictionary<City, uint> distances = new Dictionary<City, uint>();
            Dictionary<City, City> previous = new Dictionary<City, City>();

            // Priority-Queue
            SortedSet<CityScore> queue = new SortedSet<CityScore>();

            // Init distance
            distances[start] = 0;
            queue.Add(new CityScore(start, 0));

            while (queue.Count > 0)
            {
                // Pop min from Queue
                CityScore currentCD = queue.Min;
                queue.Remove(currentCD);

                if (currentCD.City == end)
                    break;

                // Split into local vars for ease-of-use
                City currCity = currentCD.City;
                uint currDist = currentCD.Score;

                foreach (Street street in currCity.Streets)
                {
                    // Find Neighbour
                    City neighbour = street.GetOtherCity(currCity);
                    uint updatedDist = currDist + street.Length;

                    if (!distances.ContainsKey(neighbour) || updatedDist < distances[neighbour])
                    {
                        // Remove old if exists
                        if (distances.ContainsKey(neighbour))
                            queue.Remove(new CityScore(neighbour, distances[neighbour]));

                        // Add distances
                        distances[neighbour] = updatedDist;
                        previous[neighbour] = currCity;

                        // Add neighbour to Queue
                        queue.Add(new CityScore(neighbour, updatedDist));
                    }
                }
            }

            if (!previous.ContainsKey(end) && start != end)
                throw new InvalidOperationException($"Could not create path between {start.Name} and {end.Name}");

            // Traverse End->Start for Path
            List<City> finalPath = new List<City>();
            City current = end;
            finalPath.Add(current);
            while (current != start)
            {
                if (!previous.ContainsKey(current))
                    break;
                current = previous[current];
                finalPath.Add(current);
            }
            // End->Start is reversed to Start->End
            finalPath.Reverse();

            // Return Path
            return new Path(finalPath.ToArray());
        }
    }
}