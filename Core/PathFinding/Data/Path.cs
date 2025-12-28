using dev.vanHoof.ManusTest.Core.Data;
using System.Linq;
using System.Text;

namespace dev.vanHoof.ManusTest.Core.Pathfinding.Data
{
    /// <summary>
    /// A Path between two <see cref="City"/>s in a remote land.
    /// <para>
    /// Paths define a route between Cities by listing the Cities one travels through.
    /// </para>
    /// </summary>
    public struct Path
    {
        /// <summary>
        /// City this Path starts at.
        /// </summary>
        public City Start => FullPath?.First();

        /// <summary>
        /// City this Path ends at.
        /// </summary>
        public City End => FullPath?.Last();

        /// <summary>
        /// Array of Cities to traverse on this Path, in order.
        /// <para>
        /// Includes <see cref="Start"/> and <see cref="End"/>.
        /// </para>
        /// </summary>
        public City[] FullPath;

        /// <summary>
        /// Constructor for Path.
        /// </summary>
        /// <param name="path">In-order list of Cities to traverse on this path</param>
        public Path(City[] path)
        {
            FullPath = path;
        }

        /// <summary>
        /// Returns [{City1},{City2},{City3}]
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < FullPath?.Length; i++)
            {
                stringBuilder.Append(FullPath[i].Name);
                if (i != FullPath.Length - 1)
                    stringBuilder.Append(',');
            }
            return $"[{stringBuilder}]";
        }
    }
}