using dev.vanHoof.ManusTest.Core.Pathfinding;

namespace dev.vanHoof.ManusTest.ConsoleApp
{
    /// <summary>
    /// Utils-Class for getting Input from the User.
    /// </summary>
    internal static class Input
    {
        /// <summary>
        /// Prompts User for an Int.
        /// <para>
        /// Infinite loop until valid input.
        /// </para>
        /// </summary>
        /// <param name="prompt">Prompt to display to User</param>
        /// <param name="min">Minimum valid value</param>
        /// <param name="max">Maximum valid value</param>
        /// <returns>Input from user</returns>
        internal static int PromptInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine();
                if (int.TryParse(input, out int value) && value >= min && value <= max)
                    return value;

                Console.WriteLine($"Invalid Input: {input}");
            }
        }

        /// <summary>
        /// Prompts User for a Boolean.
        /// <para>
        /// Infinite loop until valid input.
        /// Valid input: y/n, Y/N, Yes/No, yes/no, 1/0, true/false, True/False
        /// </para>
        /// </summary>
        /// <param name="prompt">Prompt to display to User</param>
        /// <returns>Input-Bool from user</returns>
        internal static bool PromptBool(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                string loweredInput = input.ToLower();

                switch (loweredInput)
                {
                    case "y":
                    case "1":
                    case "yes":
                    case "true":
                        return true;
                    case "n":
                    case "0":
                    case "no":
                    case "false":
                        return false;
                    default:
                        Console.WriteLine($"Invalid Input: {input}");
                        break;
                }
            }
        }

        /// <summary>
        /// Prompts User to select an Enum-Value.
        /// <para>
        /// Inifinite loop until valid input.
        /// </para>
        /// </summary>
        /// <typeparam name="T">Enum-Type</typeparam>
        /// <param name="prompt">Prompt to display to User</param>
        /// <returns>Selected Enum-Value</returns>
        internal static T PromptEnum<T>(string prompt) where T : Enum
        {
            T[] values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            Console.WriteLine(prompt);
            
            // Print enum values
            for (int i = 0; i < values.Length; i++)
                Console.WriteLine($"[{i}] {values[i]}");

            int choice = PromptInt("Select: ", 0, values.Length - 1);
            return values[choice];
        }

        /// <summary>
        /// Prompts User to select a Pathfinding-Algorithm.
        /// <para>
        /// NOTE: Returns a new uninitialized Pathfinder.
        /// </para>
        /// </summary>
        /// <returns>New uninitialized PathFinder based on User-input</returns>
        internal static IPathFinder PromptForPathfinder()
        {
            PathfindingAlgorithm currAlgorithm = Input.PromptEnum<PathfindingAlgorithm>("Select a pathfinding algorithm: ");
            return currAlgorithm.CreatePathFinder();
        }
    }
}
