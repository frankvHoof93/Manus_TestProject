using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace dev.vanHoof.ManusTest.UI
{
    /// <summary>
    /// Manages UI handling Street-Length.
    /// </summary>
    public class StreetLengthUI : MonoBehaviour
    {
        /// <summary>
        /// Whether to Randomize Length.
        /// </summary>
        public bool RandomizeLength => randomizeToggle.isOn;
        /// <summary>
        /// (Min, Max) for Length.
        /// </summary>
        public Vector2Int LengthCaps => new Vector2Int(int.Parse(ifMin.text), int.Parse(ifMax.text));

        /// <summary>
        /// Toggle for using randomized Lengths.
        /// </summary>
        [Header("Scene-Objects")]
        [SerializeField]
        private Toggle randomizeToggle;
        /// <summary>
        /// Inputfield for minimum Street-Length
        /// </summary>
        [SerializeField]
        private TMP_InputField ifMin;
        /// <summary>
        /// Inputfield for maximum Street-Length
        /// </summary>
        [SerializeField]
        private TMP_InputField ifMax;
    }
}
