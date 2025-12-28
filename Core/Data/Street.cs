using System;

namespace dev.vanHoof.ManusTest.Core.Data
{
    /// <summary>
    /// A Street connects 2 Cities in a remote land.
    /// </summary>
    public class Street : IEquatable<Street>, IComparable<Street>
    {
        /// <summary>
        /// Street-Lengths (Defaults to 1).
        /// </summary>
        public readonly uint Length;

        /// <summary>
        /// The two Cities connected by this Street.
        /// </summary>
        public readonly City[] Cities;

        /// <summary>
        /// Constructor for a Street.
        /// <para>
        /// Registers this Street to both Cities it connects to.
        /// </para>
        /// </summary>
        /// <param name="city1">First City this Street connects</param>
        /// <param name="city2">Second City this Street connects</param>
        /// <param name="length">Length of Street (defaults to 1)</param>
        public Street(City city1, City city2, uint length = 1)
        {
            Length = length;
            Cities = new City[] { city1, city2 };
            city1.AddStreet(this);
            city2.AddStreet(this);
        }

        /// <summary>
        /// Gets the other City on this Street.
        /// </summary>
        /// <param name="city">One of the Cities on this Street</param>
        /// <returns>Other City on this Street</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="city"/> is not on this Street</exception>
        public City GetOtherCity(City city)
        {
            if (Cities[0] == city)
                return Cities[1];
            if (Cities[1] == city)
                return Cities[0];
            throw new ArgumentException("City is not on this Street");
        }

        /// <summary>
        /// Compares Streets by Length.
        /// </summary>
        /// <param name="other">Street to compare to</param>
        /// <returns>Length-Comparison (-1, 0 or 1)</returns>
        public int CompareTo(Street other)
            => Length.CompareTo(other?.Length);

        /// <summary>
        /// Equality-Comparer for Streets
        /// <para>
        /// Streets are Equal if their Lengths and Cities match.
        /// The order of Cities in <see cref="Cities"/> does not matter.
        /// </para>
        /// </summary>
        /// <param name="other">Street to compare to</param>
        /// <returns>True if Length and Cities match</returns>
        public bool Equals(Street other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            if (Length != other.Length)
                return false;
            return (Cities[0].Equals(other.Cities[0]) && Cities[1].Equals(other.Cities[1]))
                || (Cities[0].Equals(other.Cities[1]) && Cities[1].Equals(other.Cities[0]));
        }
    }
}
