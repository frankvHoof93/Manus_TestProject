using dev.vanHoof.ManusTest.Core.Data;
using dev.vanHoof.ManusTest.Core.Generation;
using dev.vanHoof.ManusTest.Core.Pathfinding;
using dev.vanHoof.ManusTest.Core.Pathfinding.Data;
using System.Text;
using Path = dev.vanHoof.ManusTest.Core.Pathfinding.Data.Path;

namespace dev.vanHoof.ManusTest.ConsoleApp
{
    /// <summary>
    /// Entry-Point for Console-App
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Minimum number of cities in WorldGen.
        /// </summary>
        private const int MIN_CITIES = 2;
        /// <summary>
        /// Maximum number of cities in WorldGen.
        /// </summary>
        private const int MAX_CITIES = 1000;
        /// <summary>
        /// Minimum Length for Streets in WorldGen.
        /// </summary>
        private const int MIN_STREET_LENGTH = 1;
        /// <summary>
        /// Maximum Length for Streets in WorldGen.
        /// </summary>
        private const int MAX_STREET_LENGTH = 100_000;

        static async Task Main()
        {
            Console.WriteLine("Pathfinding Console App (FMP van Hoof - 2025)");
            Console.WriteLine();

            // Intro-Text
            string introText = "This app lets you generate a world of Cities and Streets."
                + Environment.NewLine
                + "It then lets you pick a pathfinding-algorithm"
                + Environment.NewLine
                + "to compute the shortest path between two chosen cities.";
            Console.WriteLine(introText);
            Console.WriteLine();

            // Generate World
            City[] cities = GenerateCities();
            Console.WriteLine($"Generated {cities.Length} Cities.");
            Console.WriteLine();

            // Print World to Console
            for (int i = 0; i < cities.Length; i++)
            {
                City city = cities[i];
                StringBuilder sb = new StringBuilder();
                sb.Append($"City {city.Name}  Connections: ");

                List<Street> streets = city.Streets;
                sb.Append('[');
                for (int j = 0; j < streets.Count; j++)
                {
                    Street street = streets[j];
                    City neighbour = street.GetOtherCity(city);
                    sb.Append(neighbour.Name);
                    if (j != streets.Count - 1)
                        sb.Append(',');
                }
                sb.Append(']');
                Console.WriteLine(sb.ToString());
            }
            Console.WriteLine();

            // Run Pathfinding-Loop
            await PathfindingLoop(cities);
        }

        /// <summary>
        /// Generates Cities based on User-Input.
        /// </summary>
        /// <returns>Array of generated Cities</returns>
        private static City[] GenerateCities()
        {
            int cityCount = Input.PromptInt("Number of Cities to Generate: ", MIN_CITIES, MAX_CITIES);
            bool randomStreetLengths = Input.PromptBool("Should Street-Lengths be randomized? (y/n): ");

            int minStreetLen = 1;
            int maxStreetLen = 1;
            if (randomStreetLengths)
            {
                minStreetLen = Input.PromptInt($"Minimum length for Streets ({MIN_STREET_LENGTH}-{MAX_STREET_LENGTH}): ", MIN_STREET_LENGTH, MAX_STREET_LENGTH);
                maxStreetLen = Input.PromptInt($"Maximum length for Streets ({MIN_STREET_LENGTH}-{MAX_STREET_LENGTH}): ", MIN_STREET_LENGTH, MAX_STREET_LENGTH);
                while (minStreetLen > maxStreetLen)
                {
                    Console.WriteLine("Maximum Street-Length must be greater than or equal to Minimum Street-Length.");
                    maxStreetLen = Input.PromptInt($"Minimum length for Streets ({MIN_STREET_LENGTH}-{MAX_STREET_LENGTH}): ", MIN_STREET_LENGTH, MAX_STREET_LENGTH);
                }
            }

            return CityGenerator.GenerateCities(cityCount, minStreetLen, maxStreetLen);
        }

        /// <summary>
        /// Main Pathfinding-Loop.
        /// <para>
        /// Runs infinitely until exited by User.
        /// </para>
        /// </summary>
        /// <param name="cities">All Cities in the World</param>
        /// <returns>Awaitable Task</returns>
        private static async Task PathfindingLoop(City[] cities)
        {
            IPathFinder pathFinder = Input.PromptForPathfinder();
            if (!pathFinder.Initialized)
            {
                Console.WriteLine($"Initializing Pathfinder: {pathFinder.Type}");
                await pathFinder.Initialize(cities);
            }
            Console.WriteLine($"Using algorithm {pathFinder.Type}");

            // Generate Intro-text just once.
            string pathingIntroText = $"Enter Starting City ({0}–{cities.Length - 1}), 'p' to change Pathfinding-Algorithm, or 'q' to quit:";

            while (true)
            {
                Console.WriteLine();

                // Intro-Text
                const string pathingTitle = "Pathfinding:";
                Console.WriteLine(pathingTitle);
                Console.WriteLine(pathingIntroText);

                // Input
                string? input = Console.ReadLine()?.Trim().ToLower();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                // Quit
                if (input.Equals("q"))
                    break;

                // Pathfinding-Algorithm
                if (input.Equals("p"))
                {
                    pathFinder = Input.PromptForPathfinder();
                    if (!pathFinder.Initialized)
                    {
                        Console.WriteLine($"Initializing Pathfinder: {pathFinder.Type}");
                        await pathFinder.Initialize(cities);
                    }
                    Console.WriteLine($"Using algorithm {pathFinder.Type}");
                    continue;
                }

                // Starting City
                if (!int.TryParse(input, out int startIndex)
                    || (startIndex < 0 || startIndex >= cities.Length))
                {
                    Console.WriteLine($"Invalid Starting City. Pick from ({0}-{cities.Length - 1})");
                    continue;
                }

                // Ending City
                int endIndex = Input.PromptInt(
                    $"Enter Ending City ({0}–{cities.Length - 1}): ",
                    0,
                    cities.Length - 1);

                // Get City-Instances
                City start = cities[startIndex];
                City end = cities[endIndex];

                try
                {
                    // PathFinding
                    Path path = pathFinder.FindPathBetween(start, end);
                    Console.WriteLine($"Shortest Path from City {start.Name} to City {end.Name}: {path}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}