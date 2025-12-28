using dev.vanHoof.ManusTest.Core.Data;
using dev.vanHoof.ManusTest.Core.Generation;
using dev.vanHoof.ManusTest.Core.Pathfinding;
using dev.vanHoof.ManusTest.Core.Pathfinding.Data;
using dev.vanHoof.ManusTest.WorldObjects.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace dev.vanHoof.ManusTest.WorldObjects.Managers
{
    /// <summary>
    /// Object used to store a reference to the current grid of Cities.
    /// </summary>
    public class World : MonoBehaviour
    {
        /// <summary>
        /// Event fired when World-Gen completes.
        /// </summary>
        public event Action<City[]> OnWorldGenerated;

        /// <summary>
        /// Reference to Visualizer in Scene.
        /// </summary>
        public Visualizer Visualizer => visualizer;
        /// <summary>
        /// Reference to SelectionManager in Scene.
        /// </summary>
        public SelectionManager SelectionManager => selectionManager;

        /// <summary>
        /// Visualizer in Scene.
        /// </summary>
        [Header("SceneObjects")]
        [SerializeField]
        private Visualizer visualizer;
        /// <summary>
        /// SelectionManager in Scene.
        /// </summary>
        [SerializeField]
        private SelectionManager selectionManager;

        /// <summary>
        /// Active City-Objects (Data, GameObject)
        /// </summary>
        private Dictionary<City, CityObject> cityObjects;
        /// <summary>
        /// Active Street-Objects (Data, GameObject)
        /// </summary>
        private Dictionary<Street, StreetObject> streetObjects;

        /// <summary>
        /// ASYNC: Generates a new World.
        /// </summary>
        /// <param name="cityAmount">Amount of Cities to generate.</param>
        /// <param name="minStreetLen">Minimum Length for Streets.</param>
        /// <param name="maxStreetLen">Maximum Length for Streets.</param>
        /// <param name="cancellationToken">Token used for cancelling generation</param>
        /// <returns>Awaitable Task</returns>
        public async Task AsyncGenerateWorld(int cityAmount, int minStreetLen = 1, int maxStreetLen = 1, CancellationToken cancellationToken = default)
        {
            // Unload any previous world.
            UnloadWorld();
            // Data-gen
            City[] cities = CityGenerator.GenerateCities(cityAmount, minStreetLen, maxStreetLen);
            // Object-gen
            Visualizer.PlacementResult result = await visualizer.PlaceCities(cities, cancellationToken);
            cityObjects = result.cityObjects;
            streetObjects = result.streetObjects;
            // Add OnClick-references to SelectManager
            foreach (CityObject city in cityObjects.Values)
            {
                city.OnClickCity += selectionManager.SelectCity;
            }
            OnWorldGenerated?.Invoke(cities);
        }

        /// <summary>
        /// Unloads and destroys active Cities.
        /// </summary>
        public void UnloadWorld()
        {
            if (cityObjects != null)
            {
                foreach (CityObject city in cityObjects.Values)
                {
                    city.OnClickCity -= selectionManager.SelectCity;
                    Destroy(city.gameObject);
                }
                cityObjects.Clear();
            }
            if (streetObjects != null)
            {
                foreach (StreetObject street in streetObjects.Values)
                {
                    Destroy(street.gameObject);
                }
                streetObjects.Clear();
            }
        }

        /// <summary>
        /// ASYNC: Generates a path between 2 cities.
        /// </summary>
        /// <param name="start">City to start at</param>
        /// <param name="end">City to end at</param>
        /// <param name="pathfinder">Pathfinder to use</param>
        /// <param name="cancellationToken">Token used for cancelling pathfinding</param>
        /// <returns>Path from <paramref name="start"/> to <paramref name="end"/></returns>
        public async Task<Path> GeneratePath(City start, City end, IPathFinder pathfinder, CancellationToken cancellationToken = default)
        {
            if (!pathfinder.Initialized)
                await pathfinder.Initialize(GetCities(), cancellationToken);
            Path path = pathfinder.FindPathBetween(start, end);
            await visualizer.ShowPath(path, cancellationToken);
            return path;
        }

        /// <summary>
        /// Returns all active Cities.
        /// </summary>
        public City[] GetCities()
            => cityObjects.Keys.ToArray();

        /// <summary>
        /// Returns all active CityObjects.
        /// </summary>
        public CityObject[] GetCityObjects()
            => cityObjects.Values.ToArray();

        /// <summary>
        /// Returns all active StreetObjects.
        /// </summary>
        public StreetObject[] GetStreetObjects()
            => streetObjects.Values.ToArray();

        /// <summary>
        /// Gets CityObject by City.
        /// </summary>
        public CityObject this[City city]
            => cityObjects[city];

        /// <summary>
        /// Gets StreetObject by Street.
        /// </summary>
        public StreetObject this[Street street]
            => streetObjects[street];
    }
}