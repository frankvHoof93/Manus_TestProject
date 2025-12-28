using dev.vanHoof.ManusTest.Core.Data;
using dev.vanHoof.ManusTest.Core.Pathfinding;
using dev.vanHoof.ManusTest.UI.Components;
using dev.vanHoof.ManusTest.WorldObjects.Objects;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace dev.vanHoof.ManusTest.WorldObjects.Managers
{
    /// <summary>
    /// Handles Selecting Cities.
    /// </summary>
    public class SelectionManager : MonoBehaviour
    {
        /// <summary>
        /// Reference to World-Object.
        /// </summary>
        [Header("SceneObjects")]
        [SerializeField]
        private World world;
        /// <summary>
        /// Reference to Dropdown selection of Pathfinding-Algorithm.
        /// </summary>
        [SerializeField]
        private PathfindingDropdown pathfindingDropdown;

        /// <summary>
        /// Selected "Start-City" for pathing.
        /// </summary>
        private CityObject selectedCity = null;
        /// <summary>
        /// CancelToken-Source for PathFinding-Tasks.
        /// </summary>
        private CancellationTokenSource cancelTokens;
        /// <summary>
        /// Cache of Initialized Pathfinders.
        /// </summary>
        private readonly Dictionary<PathfindingAlgorithm, IPathFinder> pathFinderCache = new Dictionary<PathfindingAlgorithm, IPathFinder>();

        /// <summary>
        /// Selects a City.
        /// <para>
        /// If second selection, runs pathfinding.
        /// </para>
        /// </summary>
        /// <param name="city">City to select</param>
        public async void SelectCity(CityObject city)
        {
            if (city == selectedCity)
            {
                DeselectCity(city);
                return;
            }

            if (selectedCity == null)
            {
                cancelTokens?.Cancel();
                cancelTokens = new CancellationTokenSource();

                // Clear any previous selections,
                // that were visible from previous path-find.
                foreach (CityObject cityObj in world.GetCityObjects())
                {
                    cityObj.Deselect();
                }
                foreach (StreetObject streetObj in world.GetStreetObjects())
                {
                    streetObj.Deselect();
                }
            }

            city.Select();
            if (selectedCity != null)
            {
                // PathFinding from SelectedCity to City.
                PathfindingAlgorithm algorithm = pathfindingDropdown.GetSelection();
                IPathFinder pathFinder = null;
                if (pathFinderCache.ContainsKey(algorithm))
                    pathFinder = pathFinderCache[algorithm];
                else
                {
                    pathFinder = algorithm.CreatePathFinder();
                    pathFinderCache[algorithm] = pathFinder;
                }

                // local ref to selectedCity-city
                City start = selectedCity.City;

                // Set selected back to NULL, so we can clear everything on next click.
                selectedCity = null;

                try
                {
                    // Generate & visualize Path.
                    await world.GeneratePath(start, city.City, pathFinder, cancelTokens.Token);
                }
                catch (OperationCanceledException)
                {
                    Debug.LogWarning("Canceled Pathfinding");
                }
            }
            else
            {
                selectedCity = city;
            }
        }

        /// <summary>
        /// Deselects City.
        /// </summary>
        /// <param name="city">City to deselect</param>
        public void DeselectCity(CityObject city)
        {
            city.Deselect();
            selectedCity = null;
        }

        /// <summary>
        /// Hook into <see cref="World.OnWorldGenerated"/>.
        /// </summary>
        private void Awake()
        {
            world.OnWorldGenerated += OnCityGenerated;
        }

        /// <summary>
        /// Remove listener on <see cref="World.OnWorldGenerated"/>.
        /// </summary>
        private void OnDestroy()
        {
            if (world != null)
                world.OnWorldGenerated -= OnCityGenerated;
            cancelTokens?.Cancel();
        }

        /// <summary>
        /// Called when new city is generated. Clears <see cref="pathFinderCache"/>.
        /// </summary>
        private void OnCityGenerated(City[] cities)
        {
            pathFinderCache.Clear();
        }
    }
}
