using dev.vanHoof.ManusTest.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dev.vanHoof.ManusTest.Core.Generation
{
    /// <summary>
    /// Generates <see cref="City"/>s and the <see cref="Street"/>s that connect them.
    /// </summary>
    public static class CityGenerator
    {
        /// <summary>
        /// Generates an array of <see cref="City"/>s connected through <see cref="Street"/>s.
        /// </summary>
        /// <param name="totalCities">Total number of Cities to generate</param>
        /// <param name="minStreetLength">Minimum length for Streets. Defaults to 1.</param>
        /// <param name="maxStreetLength">Maximum length for Streets. Defaults to 1.</param>
        /// <returns>Array of newly generated Cities</returns>
        public static City[] GenerateCities(int totalCities, int minStreetLength = 1, int maxStreetLength = 1)
        {
            List<City> result = new List<City>(totalCities);
            Random rand = new Random();

            // Create City 0.
            City lastAdded = new City("0");
            result.Add(lastAdded);

            // Create all other Cities
            for (int i = 1; i < totalCities; i++)
            {
                City newCity = new City(i.ToString());
                // Constructor adds Street to both cities
                uint streetLength = (uint)rand.Next(minStreetLength, maxStreetLength + 1);
                Street streetToLast = new Street(newCity, lastAdded, streetLength);
                if (result.Count > 1) // There is another city present outside of LastAdded
                {
                    // New city not yet in list, so we only need to filter out LastAdded.
                    City randomCity = result.Where(r => r != lastAdded).ElementAt(rand.Next(result.Count - 1));
                    streetLength = (uint)rand.Next(minStreetLength, maxStreetLength + 1);
                    Street streetToRandom = new Street(newCity, randomCity, streetLength);
                }
                // Add new City to result only after creating streets.
                result.Add(newCity);
                lastAdded = newCity;
            }

            return result.ToArray();
        }
    }
}