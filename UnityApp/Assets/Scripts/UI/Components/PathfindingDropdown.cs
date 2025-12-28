using dev.vanHoof.ManusTest.Core.Pathfinding;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace dev.vanHoof.ManusTest.UI.Components
{
    /// <summary>
    /// Auto-sets all values of <see cref="PathfindingAlgorithm"/> to <see cref="TMP_Dropdown"/>-options.
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public class PathfindingDropdown : MonoBehaviour
    {
        /// <summary>
        /// Dropdown managed by this script.
        /// </summary>
        private TMP_Dropdown dropdown;

        /// <summary>
        /// Parses selected value to <see cref="PathfindingAlgorithm"/>
        /// </summary>
        /// <returns>Parsed value, or <see cref="PathfindingAlgorithm.Dijkstra"/> if parsing failed.</returns>
        public PathfindingAlgorithm GetSelection()
        {
            if (Enum.TryParse(dropdown.options[dropdown.value].text, true, out PathfindingAlgorithm result))
            {
                return result;
            }

            Debug.LogError("Couldn't parse selected algorithm. Defaulting to Dijkstra");
            return PathfindingAlgorithm.Dijkstra;
        }

        /// <summary>
        /// Sets options on Dropdown.
        /// </summary>
        private void Awake()
        {
            dropdown = GetComponent<TMP_Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(Enum.GetNames(typeof(PathfindingAlgorithm)).ToList());
        }
    }
}
