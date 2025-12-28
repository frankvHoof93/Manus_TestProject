using dev.vanHoof.ManusTest.Core.Data;
using System;

namespace dev.vanHoof.ManusTest.Core.Pathfinding.Data
{
    /// <summary>
    /// Tuple (City, Score) used by various Pathfinding-Algorithms
    /// to link a Score (Cost, Priority, etc) to a City.
    /// </summary>
    internal readonly struct CityScore : IComparable<CityScore>
    {
        /// <summary>
        /// Score. Exact definition is Algorithm-dependent.
        /// </summary>
        public readonly uint Score;
        /// <summary>
        /// City at End.
        /// </summary>
        public readonly City City;

        public CityScore(City city, uint score)
        {
            City = city;
            Score = score;
        }

        /// <summary>
        /// Compare by Score. Breaks ties using City-name.
        /// </summary>
        public int CompareTo(CityScore other)
        {
            int cmp = Score.CompareTo(other.Score);
            if (cmp == 0)
                cmp = City.Name.CompareTo(other.City.Name); // Break ties using name
            return cmp;
        }
    }
}
