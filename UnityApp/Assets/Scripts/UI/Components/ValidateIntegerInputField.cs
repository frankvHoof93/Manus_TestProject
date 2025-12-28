using TMPro;
using UnityEngine;

namespace dev.vanHoof.ManusTest.UI.Components
{
    /// <summary>
    /// Validates Input on Integer-based <see cref="TMP_InputField"/>
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class ValidateIntegerInputField : MonoBehaviour
    {
        /// <summary>
        /// Range for valid inputs.
        /// </summary>
        public Vector2Int Range = new Vector2Int(1, int.MaxValue);

        /// <summary>
        /// Inputfield being validated.
        /// </summary>
        private TMP_InputField inputField;

        /// <summary>
        /// Adds Listener to Inputfield.
        /// </summary>
        private void Awake()
        {
            inputField = GetComponent<TMP_InputField>();

            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputField.onValueChanged.AddListener(ValidateInput);
        }

        /// <summary>
        /// Removes Listener from Inputfield.
        /// </summary>
        private void OnDestroy()
        {
            if (inputField != null)
                inputField.onValueChanged.RemoveListener(ValidateInput);
        }

        /// <summary>
        /// Validates InputField.
        /// </summary>
        /// <param name="input">New Input</param>
        private void ValidateInput(string input)
        {
            if (!int.TryParse(input, out int inputValue))
            {
                // Invalid input. Ignore it, as the InputField's ContentType should fix it.
                return;
            }

            int clampedValue = Mathf.Clamp(inputValue, Range.x, Range.y);
            if (clampedValue != inputValue)
            {
                // Set WITH notify.
                inputField.text = clampedValue.ToString();
            }
        }
    }
}
