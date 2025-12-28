using dev.vanHoof.ManusTest.Core.Pathfinding.Implementations;

namespace dev.vanHoof.ManusTest.Core.Pathfinding
{
    /// <summary>
    /// Types of PathFindingAlgorithms that are available.
    /// </summary>
    public enum PathfindingAlgorithm : short
    {
        Dijkstra = 0,
        AStar = 1,
        BellmanFord = 2,
        FloydWarshall = 3,
        Johnson = 4,
        DijkstraBidirectional = 5
    }

    /// <summary>
    /// Extension-Methods for <see cref="PathfindingAlgorithm"/>.
    /// </summary>
    public static class PathfindingAlgorithmExtensions
    {
        /// <summary>
        /// Creates a new Pathfinding-Implementation from the Algorithm-Type.
        /// </summary>
        /// <param name="algorithm">Type of Algorithm to create PathFinder for.</param>
        /// <returns>new IPathFinder() based on Algorithm-Type</returns>
        /// <exception cref="System.NotImplementedException">Thrown for Invalid Types</exception>
        public static IPathFinder CreatePathFinder(this PathfindingAlgorithm algorithm)
            => algorithm switch
            {
                PathfindingAlgorithm.Dijkstra => new DijkstraPathFinder(),
                PathfindingAlgorithm.AStar => new AStarPathfinder(),
                PathfindingAlgorithm.BellmanFord => new BellmanFordPathFinder(),
                PathfindingAlgorithm.FloydWarshall => new FloydWarshallPathfinder(),
                PathfindingAlgorithm.Johnson => new JohnsonPathFinder(),
                PathfindingAlgorithm.DijkstraBidirectional => new BidirectionalDijkstraPathFinder(),
                _ => throw new System.NotImplementedException($"No Pathfinder has been implemented for algorithm {algorithm}.")
            };
    }
}