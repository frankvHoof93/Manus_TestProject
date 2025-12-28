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
    /// <see cref="IPathFinder"/> that implements the Floyd-Warshall Algorithm.
    /// <para>
    /// https://en.wikipedia.org/wiki/Floyd%E2%80%93Warshall_algorithm
    /// </para>
    /// </summary>
    public class FloydWarshallPathfinder : IPathFinder
    {
        /// <summary>
        /// Number of times to (pre-)compute before yielding.
        /// </summary>
        private const int INIT_YIELD_COUNT = 10;

        /// <summary>
        /// Floyd-Warshall Algorithm.
        /// </summary>
        public PathfindingAlgorithm Type => PathfindingAlgorithm.FloydWarshall;

        /// <inheritdoc />
        public bool Initialized { get; private set; } = false;

        private City[] cities;
        private Dictionary<City, int> cityIndex;
        private uint[,] dist;
        private int?[,] next;

        /// <summary>
        /// Compute City-Indices
        /// </summary>
        public async Task Initialize(IEnumerable<City> allCities, CancellationToken cancellationToken = default)
        {
            cities = allCities.ToArray();
            int cityCount = cities.Length;

            cityIndex = new Dictionary<City, int>();
            dist = new uint[cityCount, cityCount];
            next = new int?[cityCount, cityCount];

            // Init-loop.
            for (int i = 0; i < cityCount; i++)
            {
                cityIndex[cities[i]] = i;

                for (int j = 0; j < cityCount; j++)
                {
                    if (i == j)
                    {
                        dist[i, j] = 0;
                        next[i, j] = null;
                    }
                    else
                    {
                        dist[i, j] = uint.MaxValue / 2;
                        next[i, j] = null;
                    }
                }
            }

            // Direct neighbours
            foreach (City city in cities)
            {
                int index = cityIndex[city];
                foreach (Street street in city.Streets)
                {
                    City neighbour = street.GetOtherCity(city);
                    int neighbourIndex = cityIndex[neighbour];
                    if (street.Length < dist[index, neighbourIndex])
                    {
                        dist[index, neighbourIndex] = street.Length;
                        next[index, neighbourIndex] = neighbourIndex;
                    }
                }
            }

            if (INIT_YIELD_COUNT > 0)
                await Task.Yield(); // Yield once before entering main loop.
            cancellationToken.ThrowIfCancellationRequested();
            int calcs = 0;

            // Main Floyd-Marshall loop.
            for (int k = 0; k < cityCount; k++)
                for (int i = 0; i < cityCount; i++)
                    for (int j = 0; j < cityCount; j++)
                        if (dist[i, k] + dist[k, j] < dist[i, j])
                        {
                            dist[i, j] = dist[i, k] + dist[k, j];
                            next[i, j] = next[i, k];

                            calcs++;
                            if (INIT_YIELD_COUNT > 0 && calcs / INIT_YIELD_COUNT > 0)
                            {
                                await Task.Yield(); // keep async-friendly
                                cancellationToken.ThrowIfCancellationRequested();
                                // Keep remainder for next cycle.
                                calcs %= INIT_YIELD_COUNT;
                            }
                        }

            // Finished Init.
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

            if (!cityIndex.ContainsKey(start) || !cityIndex.ContainsKey(end))
                throw new ArgumentException("Start or End City not in graph.");

            int indexStart = cityIndex[start];
            int indexEnd = cityIndex[end];

            if (next[indexStart, indexEnd] == null)
                throw new InvalidOperationException($"No path exists between {start.Name} and {end.Name}");

            // Reconstruct path
            List<City> path = new List<City> { start };
            int current = indexStart;
            while (current != indexEnd)
            {
                current = next[current, indexEnd].Value;
                path.Add(cities[current]);
            }

            return new Path(path.ToArray());
        }
    }
}