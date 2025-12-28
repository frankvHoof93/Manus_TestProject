using dev.vanHoof.ManusTest.Core.Data;
using dev.vanHoof.ManusTest.Core.Pathfinding.Data;
using dev.vanHoof.ManusTest.Utils;
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
    /// Visualizes <see cref="City"/>s, <see cref="Street"/>s and <see cref="Path"/>s.
    /// </summary>
    public class Visualizer : MonoBehaviour
    {
        /// <summary>
        /// Result of visualizing worldgen. Contains references to GameObject for generated world.
        /// </summary>
        public struct PlacementResult
        {
            public Dictionary<City, CityObject> cityObjects;
            public Dictionary<Street, StreetObject> streetObjects;
        }

        /// <summary>
        /// Reference to <see cref="World"/> in Scene.
        /// </summary>
        [Header("SceneObjects")]
        [SerializeField]
        private World world;

        /// <summary>
        /// Prefab for spawning City-Objects in the Scene.
        /// </summary>
        [Header("Prefabs")]
        [SerializeField]
        private CityObject cityPrefab;
        /// <summary>
        /// Prefab for spawning Street-Objects int the Scene.
        /// </summary>
        [SerializeField]
        private StreetObject streetPrefab;

        /// <summary>
        /// Base-number of Candidates to try per step when positioning Cities.
        /// </summary>
        [Header("Settings")]
        [SerializeField]
        private uint baseCandidatesPerStep = 30;
        /// <summary>
        /// Scaled-number of Candidates to try per step when positioning Cities.
        /// <para>
        /// Scales with total number of Cities.
        /// </para>
        /// </summary>
        [SerializeField]
        private uint scaledCandidatesPerStep = 6;

        private float targetDistance;
        private Vector2 placementCenter;
        private float placementRadius;

        private float spawnDelay;
        private float pathingDelay;

        private Transform cityParent;
        private Transform streetParent;

        /// <summary>
        /// ASYNC: Spawns Cities & Roads.
        /// </summary>
        /// <param name="cities">Cities in World.</param>
        /// <param name="cancellationToken">Token used to cancel spawning</param>
        /// <returns>PlacementResult with newly spawned Objects</returns>
        public async Task<PlacementResult> PlaceCities(City[] cities, CancellationToken cancellationToken = default)
        {
            if (cities == null || cities.Length == 0)
                return default;

            // Compute "Area per City"
            Rect placementRect = (world.transform as RectTransform).rect;
            placementCenter = placementRect.center;
            placementRadius = Mathf.Min(placementRect.width * world.transform.lossyScale.x, placementRect.height * world.transform.lossyScale.y) * .5f;

            float area = Mathf.PI * placementRadius * placementRadius;
            targetDistance = Mathf.Sqrt(area / cities.Length);

            // Compute CandidatesPerStep
            uint candidatesPerStep = CalcCandidatesPerStep(cities.Length);

            // Check if cancelled
            cancellationToken.ThrowIfCancellationRequested();

            // Init variables
            List<Vector2> placedPositions = new List<Vector2>();
            Dictionary<City, CityObject> cityObjects = new Dictionary<City, CityObject>(cities.Length);
            Dictionary<Street, StreetObject> streetObjects = new Dictionary<Street, StreetObject>(cities.Length * 2);

            // Place first City at center
            SpawnCity(cities[0], placementCenter, placedPositions, cityObjects, streetObjects);

            // Place other Cities
            for (int i = 1; i < cities.Length; i++)
            {
                // Check if cancelled
                cancellationToken.ThrowIfCancellationRequested();

                // Add delay for nice visualization
                if (spawnDelay > 0f)
                    await Task.Delay(Mathf.RoundToInt(spawnDelay * 1000f), cancellationToken);
                AddCity(cities[i], candidatesPerStep, placedPositions, cityObjects, streetObjects);
            }

            return new PlacementResult
            {
                cityObjects = cityObjects,
                streetObjects = streetObjects
            };
        }

        /// <summary>
        /// ASYNC: Visualizes a Path by selecting the Cities and Streets along it.
        /// </summary>
        /// <param name="path">Path to visualize</param>
        /// <param name="cancellationToken">Token used to cancel visualization</param>
        /// <returns>awaitable Task</returns>
        public async Task ShowPath(Path path, CancellationToken cancellationToken = default)
        {
            City[] cities = path.FullPath;

            for (int i = 0; i < cities.Length; i++)
            {
                City currCity = cities[i];
                // Select current city.
                world[currCity].Select();

                // Add delay for nice visualization
                if (pathingDelay > 0)
                    await Task.Delay(Mathf.RoundToInt(pathingDelay * 1000), cancellationToken);

                // Find road to next city.
                if (i < cities.Length - 1)
                {
                    City nextCity = cities[i + 1];
                    Street street = currCity.Streets.Find(s => s.Cities[0] == nextCity || s.Cities[1] == nextCity);
                    if (street == null)
                        throw new InvalidOperationException($"Ran into an error finding Street between {currCity.Name} and {nextCity.Name}");
                    world[street].Select();

                    // Add delay for nice visualization
                    if (pathingDelay > 0)
                        await Task.Delay(Mathf.RoundToInt(pathingDelay * 1000), cancellationToken);
                }
            }
        }

        /// <summary>
        /// Updates Delays used when visualizing.
        /// </summary>
        /// <param name="spawnDelay">Delay used when Spawning</param>
        /// <param name="pathingDelay">Delay used when Pathing</param>
        public void SetDelays(float spawnDelay, float pathingDelay)
        {
            this.spawnDelay = spawnDelay;
            this.pathingDelay = pathingDelay;
        }

        /// <summary>
        /// Init Transform-hierarchy.
        /// </summary>
        private void Awake()
        {
            Transform streets = new GameObject("Streets", typeof(RectTransform)).transform;
            streets.SetParent(world.transform);
            streets.localPosition = Vector3.zero;
            streetParent = streets;

            Transform cities = new GameObject("Cities", typeof(RectTransform)).transform;
            cities.SetParent(world.transform);
            cities.localPosition = Vector3.zero;
            cityParent = cities;
        }

        /// <summary>
        /// Adds City to World.
        /// </summary>
        /// <param name="toAdd">Data for City</param>
        /// <param name="candidatesPerStep">Position-candidates to attempt per step</param>
        /// <param name="placedPositions">List of positions of already placed Cities.</param>
        /// <param name="cityObjects">Reference-Dictionary of City-Objects</param>
        /// <param name="streetObjects">Reference-Dictionary of Street-Objects</param>
        private void AddCity(City toAdd, uint candidatesPerStep, List<Vector2> placedPositions, Dictionary<City, CityObject> cityObjects, Dictionary<Street, StreetObject> streetObjects)
        {
            // Place City in World based on Poisson-Disk sampling.
            Vector2 bestCandidate = Vector2.zero;
            float bestScore = float.NegativeInfinity;

            for (int i = 0; i < candidatesPerStep; i++)
            {
                Vector2 candidate = MathUtils.RandomPointInCircle(placementCenter, placementRadius);

                float nearest = float.MaxValue;
                foreach (Vector2 existing in placedPositions)
                {
                    float dist = Vector2.Distance(candidate, existing);
                    if (dist < nearest)
                        nearest = dist;
                }

                // Score closeness based on target-spacing.
                // Negative to minimize error (max score = min error).
                float score = -Mathf.Abs(nearest - targetDistance);

                // Small noise for visual flair.
                score += UnityEngine.Random.Range(-.05f, .05f);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestCandidate = candidate;
                }
            }

            SpawnCity(toAdd, bestCandidate, placedPositions, cityObjects, streetObjects);
        }

        /// <summary>
        /// Spawns a city in the world.
        /// </summary>
        /// <param name="city">Data for City</param>
        /// <param name="localPos">LocalPosition for city</param>
        /// <param name="placedPositions">List of positions of placed cities</param>
        /// <param name="cityObjects">Reference-Dictionary of CityObjects</param>
        /// <param name="streetObjects">Reference-Dictionary of StreetObjects</param>
        private void SpawnCity(City city, Vector2 localPos, List<Vector2> placedPositions, Dictionary<City, CityObject> cityObjects, Dictionary<Street, StreetObject> streetObjects)
        {
            CityObject newCity = Instantiate(cityPrefab, cityParent);
            newCity.Init(city, localPos);
            cityObjects.Add(city, newCity);
            placedPositions.Add(localPos);

            // Spawn Streets
            foreach (Street street in city.Streets)
            {
                // NOTE: Only spawn Street if BOTH cities have been spawned.
                // This ensures we only spawn a Street when the 2nd City for that 
                // Street is spawned, thereby skipping duplicates 
                // AND ensuring both City-References exist in the world.
                City[] citiesForStreet = street.Cities;
                if (citiesForStreet.All(city => cityObjects.ContainsKey(city)))
                {
                    StreetObject newStreet = Instantiate(streetPrefab, streetParent);
                    newStreet.Init(street, cityObjects);
                    streetObjects.Add(street, newStreet);
                }
            }
        }

        /// <summary>
        /// Calculates candidates to use per step based on total number of cities.
        /// </summary>
        /// <param name="totalCities">Total number of cities in world</param>
        private uint CalcCandidatesPerStep(int totalCities)
        {
            return (uint)Mathf.Clamp(
                baseCandidatesPerStep +
                Mathf.RoundToInt(scaledCandidatesPerStep * Mathf.Sqrt(totalCities)),
            25, 100);
        }
    }
}
