using dev.vanHoof.ManusTest.Core.Data;
using dev.vanHoof.ManusTest.Core.Pathfinding.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Path = dev.vanHoof.ManusTest.Core.Pathfinding.Data.Path;

namespace dev.vanHoof.ManusTest.Core.Pathfinding
{
    /// <summary>
    /// Interface for classes implementing a Pathfinding-Algorithm.
    /// </summary>
    public interface IPathFinder
    {
        /// <summary>
        /// Type of Algorithm implemented by Class.
        /// </summary>
        PathfindingAlgorithm Type { get; }

        /// <summary>
        /// Whether the Pathfinder has ran <see cref="Initialize(IEnumerable{City})"/>.
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// Some Pathfinding-Algorithms require precomputed heuristics for optimization.
        /// This Method is here to compute those heuristics.
        /// <para>
        /// Init is only required once per Graph of Cities.
        /// </para>
        /// </summary>
        /// <param name="allCities">All Cities in the World</param>
        /// <returns>Awaitable Task</returns>
        Task Initialize(IEnumerable<City> allCities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds a Path between two cities.
        /// </summary>
        /// <param name="start">City to start at</param>
        /// <param name="end">City to end at</param>
        /// <returns>Path between Cities</returns>
        Path FindPathBetween(City start, City end);
    }
}
