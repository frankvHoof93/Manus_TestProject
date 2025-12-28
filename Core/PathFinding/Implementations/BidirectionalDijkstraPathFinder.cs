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
    /// <see cref="IPathFinder"/> that implements the Dijkstra Algorithm from both ends.
    /// </summary>
    public class BidirectionalDijkstraPathFinder : IPathFinder
    {
        /// <summary>
        /// Maintains variables for one side of the search.
        /// </summary>
        private class SearchFront
        {
            public Dictionary<City, uint> Dist { get; } = new Dictionary<City, uint>();
            public Dictionary<City, City> Prev { get; } = new Dictionary<City, City>();
            public SortedSet<CityScore> Queue { get; } = new SortedSet<CityScore>();
            public HashSet<City> Visited { get; } = new HashSet<City>();

            public SearchFront(City start)
            {
                Dist[start] = 0;
                Queue.Add(new CityScore(start, 0));
            }
        }

        /// <summary>
        /// Bi-directional Dijkstra's Algorithm.
        /// </summary>
        public PathfindingAlgorithm Type => PathfindingAlgorithm.DijkstraBidirectional;

        /// <summary>
        /// Always returns True. Dijkstra does not require Initialization.
        /// </summary>
        public bool Initialized => true;

        /// <summary>
        /// No Implementation. Dijkstra does not require Initialization.
        /// </summary>
        public Task Initialize(IEnumerable<City> allCities, CancellationToken cancellationToken = default)
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

            SearchFront forward = new SearchFront(start);
            SearchFront backward = new SearchFront(end);

            City meetingNode = null;
            while (forward.Queue.Count > 0 && backward.Queue.Count > 0)
            {
                DoStep(forward, backward, ref meetingNode);
                if (meetingNode != null)
                    break;

                DoStep(backward, forward, ref meetingNode);
                if (meetingNode != null)
                    break;
            }

            if (meetingNode == null)
                throw new InvalidOperationException("No path exists between start and end.");

            // Reconstruct path
            List<City> path = new List<City>();
            Stack<City> forwardStack = new Stack<City>();

            City currentCity = meetingNode;
            while (currentCity != start)
            {
                forwardStack.Push(currentCity);
                currentCity = forward.Prev[currentCity];
            }

            path.Add(start);
            while (forwardStack.Count > 0)
                path.Add(forwardStack.Pop());

            currentCity = meetingNode;
            while (currentCity != end)
            {
                currentCity = backward.Prev[currentCity];
                path.Add(currentCity);
            }

            return new Path(path.ToArray());
        }

        private void DoStep(SearchFront currentFront, SearchFront otherFront, ref City meetingPoint)
        {
            if (currentFront.Queue.Count == 0)
                return;

            CityScore current = currentFront.Queue.Min;
            currentFront.Queue.Remove(current);

            if (currentFront.Visited.Contains(current.City))
                return;
            currentFront.Visited.Add(current.City);

            if (otherFront.Visited.Contains(current.City))
            {
                meetingPoint = current.City;
                return;
            }

            foreach (Street street in current.City.Streets)
            {
                City neighbor = street.GetOtherCity(current.City);
                uint dist = currentFront.Dist[current.City] + street.Length;

                if (!currentFront.Dist.TryGetValue(neighbor, out uint existing) || dist < existing)
                {
                    if (currentFront.Dist.ContainsKey(neighbor))
                        currentFront.Queue.Remove(new CityScore(neighbor, existing));

                    currentFront.Dist[neighbor] = dist;
                    currentFront.Prev[neighbor] = current.City;
                    currentFront.Queue.Add(new CityScore(neighbor, dist));
                }
            }
        }
    }
}