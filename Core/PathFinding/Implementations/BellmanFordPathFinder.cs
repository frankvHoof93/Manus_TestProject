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
    /// <see cref="IPathFinder"/> that implements the Bellman-Ford Algorithm.
    /// <para>
    /// https://en.wikipedia.org/wiki/Bellman%E2%80%93Ford_algorithm
    /// </para>
    /// </summary>
    public class BellmanFordPathFinder : IPathFinder
    {
        /// <summary>
        /// Bellman-Ford Algorithm.
        /// </summary>
        public PathfindingAlgorithm Type => PathfindingAlgorithm.BellmanFord;

        /// <summary>
        /// Always returns True. Bellman-Ford does not require Initialization.
        /// </summary>
        public bool Initialized => true;

        /// <summary>
        /// No Implementation. Bellman-Ford does not require Initialization.
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

            // Init set of all reachable cities.
            HashSet<City> allCities = GetReachableCities(start);
            int cityCount = allCities.Count;

            // Create List of Streets (optimizes away nested fetch)
            List<Street> allStreets = allCities.SelectMany(c => c.Streets).Distinct().ToList();

            // Initialize vars
            Dictionary<City, int> distances = allCities.ToDictionary(c => c, c => int.MaxValue);
            Dictionary<City, City> previous = new Dictionary<City, City>();

            distances[start] = 0;

            // Relax all edges up to cityCount - 1 times
            for (int i = 0; i < cityCount - 1; i++)
            {
                bool updated = false;

                foreach (var street in allStreets)
                {
                    City u = street.Cities[0];
                    City v = street.Cities[1];

                    // Relax u -> v
                    if (distances[u] != int.MaxValue && distances[u] + street.Length < distances[v])
                    {
                        distances[v] = (int)(distances[u] + street.Length);
                        previous[v] = u;
                        updated = true;
                    }

                    // Relax v -> u (undirected graph)
                    if (distances[v] != int.MaxValue && distances[v] + street.Length < distances[u])
                    {
                        distances[u] = (int)(distances[v] + street.Length);
                        previous[u] = v;
                        updated = true;
                    }
                }

                // Early exit if no distance changed
                if (!updated)
                    break;
            }

            // Check for negative cycles (optional)
            foreach (Street street in allStreets)
            {
                City u = street.Cities[0];
                City v = street.Cities[1];

                if ((distances[u] != int.MaxValue && distances[u] + street.Length < distances[v]) ||
                    (distances[v] != int.MaxValue && distances[v] + street.Length < distances[u]))
                {
                    throw new InvalidOperationException("Graph contains a negative-weight cycle");
                }
            }

            if (!previous.ContainsKey(end))
                throw new InvalidOperationException($"Could not create path between {start.Name} and {end.Name}");

            // Reconstruct path
            List<City> path = new List<City>();
            City curr = end;
            path.Add(curr);
            while (curr != start)
            {
                curr = previous[curr];
                path.Add(curr);
            }
            path.Reverse();
            return new Path(path.ToArray());
        }

        /// <summary>
        /// Creates set of all cities reachable from <paramref name="city"/>.
        /// </summary>
        /// <param name="city">City to search from</param>
        /// <returns>Set of all reachable Cities</returns>
        private HashSet<City> GetReachableCities(City city)
        {
            HashSet<City> visited = new HashSet<City>();
            Queue<City> queue = new Queue<City>();
            queue.Enqueue(city);

            while (queue.Count > 0)
            {
                City curr = queue.Dequeue();
                foreach (Street street in curr.Streets)
                {
                    City neighbour = street.GetOtherCity(curr);
                    if (!visited.Contains(neighbour))
                    {
                        visited.Add(neighbour);
                        queue.Enqueue(neighbour);
                    }
                }
            }

            return visited;
        }
    }
}