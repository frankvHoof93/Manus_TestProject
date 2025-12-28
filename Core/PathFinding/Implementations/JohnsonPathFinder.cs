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
    /// <see cref="IPathFinder"/> that implements Johnson's Algorithm.
    /// <para>
    /// https://en.wikipedia.org/wiki/Johnson%27s_algorithm
    /// </para>
    /// </summary>
    public class JohnsonPathFinder : IPathFinder
    {
        /// <summary>
        /// Number of times to (pre-)compute before yielding.
        /// </summary>
        private const int INIT_YIELD_COUNT = 10;

        /// <summary>
        /// Johnson's Algorithm.
        /// </summary>
        public PathfindingAlgorithm Type => PathfindingAlgorithm.Johnson;

        /// <inheritdoc />
        public bool Initialized { get; private set; } = false;

        private City[] cities;
        private Dictionary<City, int> cityIndex;
        private uint[,] dist;
        private int?[,] next;

        /// <summary>
        /// Compute City-Indices.
        /// </summary>
        public async Task Initialize(IEnumerable<City> allCities, CancellationToken cancellationToken = default)
        {
            cities = allCities.ToArray();
            int cityCount = cities.Length;

            cityIndex = new Dictionary<City, int>();

            for (int i = 0; i < cityCount; i++)
                cityIndex[cities[i]] = i;

            // Add temp vertex S
            // Use anonymous object instead of Street,
            // as the Street-constructor self-registers & changes the graph.
            List<(City city1, City city2, uint Length)> allEdges = cities
                .SelectMany(c => c.Streets)
                .Select(s => (city1: s.Cities[0], city2: s.Cities[1], Length: s.Length))
                .Distinct()
                .ToList();

            City tempVertex = new City("__temp__");
            List<(City city1, City city2, uint len)> tempEdges = cities.Select(c => (city1: tempVertex, city2: c, len: 0u)).ToList();
            allEdges.AddRange(tempEdges);

            // Map to indices
            List<City> cityList = cities.ToList();
            cityList.Add(tempVertex);
            var cityIndices = cityList.Select((c, i) => (City: c, Index: i)).ToDictionary(x => x.City, x => x.Index);

            // Run Bellman-Ford from S
            int[] distBF = new int[cityCount + 1];
            for (int i = 0; i < cityCount + 1; i++)
                distBF[i] = int.MaxValue / 2;
            distBF[cityCount] = 0; // Index of vertex S.

            // Track calculations per yield.
            int calcs = 0;

            // Relax all edges n times
            for (int i = 0; i < cityCount; i++)
            {
                bool updated = false;
                foreach (var street in allEdges)
                {
                    int u = cityIndices[street.city1];
                    int v = cityIndices[street.city2];
                    int len = (int)street.Length;

                    if (distBF[u] + len < distBF[v])
                    {
                        distBF[v] = distBF[u] + len;
                        updated = true;
                    }

                    if (distBF[v] + len < distBF[u])
                    {
                        distBF[u] = distBF[v] + len;
                        updated = true;
                    }

                    calcs++;
                    if (INIT_YIELD_COUNT > 0 && calcs / INIT_YIELD_COUNT > 0)
                    {
                        await Task.Yield();
                        cancellationToken.ThrowIfCancellationRequested();
                        calcs %= INIT_YIELD_COUNT;
                    }
                }

                if (!updated)
                    break;
            }

            // Assign heuristics.
            int[] heuristics = new int[cityCount];
            for (int i = 0; i < cityCount; i++)
                heuristics[i] = distBF[i];

            // Step 3: Reweight edges
            Dictionary<(int u, int v), uint> reweightedEdges = new Dictionary<(int, int), uint>();

            foreach (var street in allEdges.Where(s => s.city1 != tempVertex && s.city2 != tempVertex))
            {
                int u = cityIndices[street.city1];
                int v = cityIndices[street.city2];

                uint wPrime = (uint)(street.Length + heuristics[u] - heuristics[v]);
                reweightedEdges[(u, v)] = wPrime;
                reweightedEdges[(v, u)] = wPrime; // Undirected graph.
            }

            if (INIT_YIELD_COUNT > 0)
            {
                await Task.Yield(); // Yield once before main Dijkstra loop.
                cancellationToken.ThrowIfCancellationRequested();
            }
            // Step 4: Dijkstra from each vertex.
            dist = new uint[cityCount, cityCount];
            next = new int?[cityCount, cityCount];

            calcs = 0;
            for (int src = 0; src < cityCount; src++)
            {
                var gScore = new uint[cityCount];
                var prev = new int?[cityCount];

                for (int i = 0; i < cityCount; i++)
                    gScore[i] = uint.MaxValue / 2;
                gScore[src] = 0;

                SortedSet<(uint score, int city)> queue = new SortedSet<(uint, int)>();
                queue.Add((0, src));

                while (queue.Count > 0)
                {
                    (uint score, int city) current = queue.Min;
                    queue.Remove(current);
                    int u = current.city;
                    uint distU = current.score;

                    for (int v = 0; v < cityCount; v++)
                    {
                        if (reweightedEdges.TryGetValue((u, v), out uint w))
                        {
                            uint tentative = distU + w;
                            if (tentative < gScore[v])
                            {
                                queue.Remove((gScore[v], v));
                                gScore[v] = tentative;
                                prev[v] = u;
                                queue.Add((gScore[v], v));
                            }
                        }
                    }

                    calcs++;
                    if (INIT_YIELD_COUNT > 0 && calcs / INIT_YIELD_COUNT > 0)
                    {
                        await Task.Yield();
                        cancellationToken.ThrowIfCancellationRequested();
                        calcs %= INIT_YIELD_COUNT;
                    }
                }

                // Recover original distances
                for (int v = 0; v < cityCount; v++)
                {
                    dist[src, v] = gScore[v] + (uint)heuristics[v] - (uint)heuristics[src];
                    next[src, v] = prev[v];
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
            List<City> path = new List<City>();
            int current = indexEnd;
            path.Add(cities[current]);

            while (current != indexStart)
            {
                int? predecessor = next[indexStart, current];
                if (predecessor == null)
                    throw new InvalidOperationException("Path reconstruction failed due to missing predecessor.");

                current = predecessor.Value;
                path.Add(cities[current]);
            }

            path.Reverse(); // Reverse to get start -> end

            return new Path(path.ToArray());
        }
    }
}