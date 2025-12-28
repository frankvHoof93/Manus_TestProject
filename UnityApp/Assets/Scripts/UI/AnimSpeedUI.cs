using dev.vanHoof.ManusTest.WorldObjects.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace dev.vanHoof.ManusTest.UI
{
    /// <summary>
    /// Manages UI handling Animation-Speed.
    /// </summary>
    public class AnimSpeedUI : MonoBehaviour
    {
        /// <summary>
        /// Delay (in Seconds) used during spawning of Cities.
        /// </summary>
        public float SpawnDelay => sldSpawning.value;
        /// <summary>
        /// Delay (in Seconds) used when animating a Path.
        /// </summary>
        public float PathingDelay => sldPathing.value;

        /// <summary>
        /// Reference to <see cref="World"/> in Scene.
        /// </summary>
        [Header("Managers")]
        [SerializeField]
        private World world;

        /// <summary>
        /// Slider for SpawnDelay.
        /// </summary>
        [Header("Scene-Objects")]
        [SerializeField]
        private Slider sldSpawning;
        /// <summary>
        /// Slider for PathingDelay.
        /// </summary>
        [SerializeField]
        private Slider sldPathing;

        /// <summary>
        /// Default SpawningDelay.
        /// </summary>
        [Header("Settings")]
        [SerializeField]
        [Range(0f, 1f)]
        private float defaultSpawningDelay = 0.1f;
        /// <summary>
        /// Default PathingDelay.
        /// </summary>
        [SerializeField]
        [Range(0f, 1f)]
        private float defaultPathingDelay = 0.1f;

        /// <summary>
        /// Init default values & add event-handlers.
        /// </summary>
        private void Awake()
        {
            sldSpawning.value = defaultSpawningDelay;
            sldPathing.value = defaultPathingDelay;
            world.Visualizer.SetDelays(sldSpawning.value, sldPathing.value);
            sldSpawning.onValueChanged.AddListener(OnSliderChanged);
            sldPathing.onValueChanged.AddListener(OnSliderChanged);
        }

        /// <summary>
        /// Remove event-handlers.
        /// </summary>
        private void OnDestroy()
        {
            if (sldSpawning != null)
                sldSpawning.onValueChanged.RemoveListener(OnSliderChanged);
            if (sldPathing != null)
                sldPathing.onValueChanged.RemoveListener(OnSliderChanged);
        }

        /// <summary>
        /// Event-handler for Slider-updates.
        /// Pushes both values to Visualizer.
        /// </summary>
        private void OnSliderChanged(float newValue)
        {
            world.Visualizer.SetDelays(sldSpawning.value, sldPathing.value);
        }
    }
}
