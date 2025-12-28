using dev.vanHoof.ManusTest.WorldObjects.Managers;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

namespace dev.vanHoof.ManusTest.UI
{
    /// <summary>
    /// Manages UI handling City-Generation.
    /// </summary>
    public class GenerateUI : MonoBehaviour
    {
        /// <summary>
        /// Reference to World-Object.
        /// </summary>
        [Header("Managers")]
        [SerializeField]
        private World world;
        /// <summary>
        /// Reference to StreetLengthUI.
        /// </summary>
        [SerializeField]
        private StreetLengthUI streetLengthUI;

        /// <summary>
        /// Reference to InputField for number of cities to generate.
        /// </summary>
        [Header("Scene-Objects")]
        [SerializeField]
        private TMP_InputField ifCityCount;

        /// <summary>
        /// Number of cities to generate.
        /// </summary>
        private int cityAmount = 20;
        /// <summary>
        /// TokenSource for cancelling city-generation.
        /// </summary>
        private CancellationTokenSource cancelTokens;

        /// <summary>
        /// Generates a new World.
        /// </summary>
        public async void Generate()
        {
            cancelTokens?.Cancel();
            cancelTokens = new CancellationTokenSource();

            int minStreetLen = 1;
            int maxStreetLen = 1;
            if (streetLengthUI.RandomizeLength)
            {
                Vector2Int caps = streetLengthUI.LengthCaps;
                minStreetLen = caps.x;
                maxStreetLen = caps.y;
            }

            try
            {
                await world.AsyncGenerateWorld(cityAmount, minStreetLen, maxStreetLen, cancelTokens.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("Canceled City Generation");
            }
        }

        /// <summary>
        /// Updates <see cref="cityAmount"/>
        /// </summary>
        /// <param name="change">Change to apply (-1, 1)</param>
        public void UpdateAmount(int change)
        {
            cityAmount += change;
            if (cityAmount < 1)
                cityAmount = 1;
            ifCityCount.text = cityAmount.ToString();
        }

        /// <summary>
        /// Sets <see cref="cityAmount"/>
        /// </summary>
        /// <param name="amount">Amount to set</param>
        public void SetAmount(string amount)
            => SetAmount(int.Parse(amount));

        /// <summary>
        /// Sets <see cref="cityAmount"/>
        /// </summary>
        /// <param name="amount">Amount to set</param>
        public void SetAmount(int amount)
        {
            if (amount < 1)
                cityAmount = 1;
            else
                cityAmount = amount;
            ifCityCount.SetTextWithoutNotify(cityAmount.ToString());
        }

        /// <summary>
        /// Init text.
        /// </summary>
        private void Awake()
        {
            ifCityCount.text = cityAmount.ToString();
        }
    }
}
