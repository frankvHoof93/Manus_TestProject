using dev.vanHoof.ManusTest.Core.Data;
using Radishmouse;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace dev.vanHoof.ManusTest.WorldObjects.Objects
{
    /// <summary>
    /// Visual representation of a <see cref="Street"/>.
    /// <para>
    /// Decoupled from Data-class as that can be used independently,
    /// and doesn't need to be a MonoBehaviour.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(UILineRenderer))]
    public class StreetObject : MonoBehaviour, ISelection
    {
        /// <summary>
        /// Data-Object for Street.
        /// </summary>
        public Street Street { get; private set; }

        /// <summary>
        /// Text to display Length.
        /// </summary>
        [Header("SceneObjects")]
        [SerializeField]
        private TMP_Text txtLength;

        /// <summary>
        /// Color to use on LineRenderer when Selected.
        /// </summary>
        [Header("Settings")]
        [SerializeField]
        private Color selectedColor = Color.blue;

        /// <summary>
        /// Color to use on LineRenderer when not Selected.
        /// </summary>
        private Color defaultColor;
        /// <summary>
        /// Renderer for Street.
        /// </summary>
        private UILineRenderer lineRenderer;

        /// <summary>
        /// Initializes StreetObject with Data.
        /// </summary>
        /// <param name="street">Data for Street.</param>
        /// <param name="referenceDict">Reference-Dictionary for Cities</param>
        public void Init(Street street, Dictionary<City, CityObject> referenceDict)
        {
            gameObject.name = $"Street({street.Cities[0].Name}-{street.Cities[1].Name})[{street.Length}]";
            Street = street;
            Vector2 pointA = referenceDict[street.Cities[0]].transform.localPosition;
            Vector2 pointB = referenceDict[street.Cities[1]].transform.localPosition;

            lineRenderer.points = new Vector2[]
            {
                pointA,
                pointB
            };

            // Activate Length-Text if Length > 1.
            bool textActive = street.Length > 1;
            txtLength.gameObject.SetActive(textActive);
            if (textActive)
            {
                txtLength.text = street.Length.ToString();
                txtLength.transform.localPosition = (pointA + pointB) * .5f; 
            }
            Deselect();
        }

        /// <inheritdoc/>
        public void Select()
        {
            lineRenderer.color = selectedColor;
        }

        /// <inheritdoc/>
        public void Deselect()
        {
            lineRenderer.color = defaultColor;
        }

        /// <summary>
        /// Initializes internal references.
        /// </summary>
        private void Awake()
        {
            lineRenderer = GetComponent<UILineRenderer>();
            defaultColor = lineRenderer.color;
        }
    }
}
