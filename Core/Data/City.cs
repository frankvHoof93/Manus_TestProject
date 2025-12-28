using System.Collections.Generic;

namespace dev.vanHoof.ManusTest.Core.Data
{
    /// <summary>
    /// A city of people in a remote land.
    /// </summary>
    public class City
    {
        /// <summary>
        /// Name of City (defaults to index)
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Streets connecting this City to other Cities.
        /// </summary>
        public readonly List<Street> Streets;

        /// <summary>
        /// Constructor for City.
        /// <para>
        /// Cities are initialized without any Streets.
        /// </para>
        /// </summary>
        /// <param name="name">Name for City</param>
        public City(string name)
        {
            Name = name;
            Streets = new List<Street>();
        }

        /// <summary>
        /// Adds a Street to this City.
        /// </summary>
        /// <param name="street"></param>
        public void AddStreet(Street street)
        {
            Streets.Add(street);
        }
    }
}
