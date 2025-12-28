using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace dev.vanHoof.ManusTest.UI.Components
{
    /// <summary>
    /// Updates <see cref="TMP_Text"/> based on <see cref="Slider"/>-value.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class SliderText : MonoBehaviour
    {
        /// <summary>
        /// Slider to hook into.
        /// </summary>
        [Header("SceneObjects")]
        [SerializeField]
        private Slider slider;

        /// <summary>
        /// Number of displayed decimals.
        /// </summary>
        [Header("Settings")]
        [SerializeField]
        private uint numDecimals = 2;

        /// <summary>
        /// Text this script displays to.
        /// </summary>
        private TMP_Text text;

        /// <summary>
        /// Adds Listener to Slider.
        /// </summary>
        private void Awake()
        {
            text = GetComponent<TMP_Text>();
            slider.onValueChanged.AddListener(OnChangedValue);
            // Set initial value.
            OnChangedValue(slider.value);
        }

        /// <summary>
        /// Remove Listener from Slider.
        /// </summary>
        private void OnDestroy()
        {
            if (slider != null)
                slider.onValueChanged.RemoveListener(OnChangedValue);
        }

        /// <summary>
        /// Updates Text with new Value.
        /// </summary>
        /// <param name="newValue">New value on Slider</param>
        private void OnChangedValue(float newValue)
        {
            text.text = newValue.ToString($"n{numDecimals}");
        }
    }
}
