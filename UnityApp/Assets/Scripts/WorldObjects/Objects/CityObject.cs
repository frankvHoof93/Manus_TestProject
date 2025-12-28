using dev.vanHoof.ManusTest.Core.Data;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace dev.vanHoof.ManusTest.WorldObjects.Objects
{
    /// <summary>
    /// Visual representation of a <see cref="City"/>.
    /// <para>
    /// Decoupled from Data-class as that can be used independently,
    /// and doesn't need to be a MonoBehaviour.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class CityObject : MonoBehaviour, IPointerClickHandler, ISelection
    {
        /// <summary>
        /// Event fired when user Clicks on this City.
        /// </summary>
        public event Action<CityObject> OnClickCity;

        /// <summary>
        /// Data-Object for City.
        /// </summary>
        public City City { get; private set; }

        /// <summary>
        /// Text to display City-Name.
        /// </summary>
        [Header("SceneObjects")]
        [SerializeField]
        private TMP_Text txtName;

        /// <summary>
        /// Color to use on Sprite when Selected.
        /// </summary>
        [Header("Settings")]
        [SerializeField]
        private Color selectedColor = Color.blue;

        /// <summary>
        /// Default Color on Sprite when not Selected.
        /// </summary>
        private Color defaultColor;
        /// <summary>
        /// Sprite-Image.
        /// </summary>
        private Image img;

        /// <summary>
        /// Initializes CityObject with Data.
        /// </summary>
        /// <param name="city">Data for City.</param>
        /// <param name="localPosition">Position for CityObject.</param>
        public void Init(City city, Vector2 localPosition)
        {
            gameObject.name = $"City({city.Name})";
            City = city;
            txtName.text = city.Name;
            transform.localPosition = localPosition;
            Deselect();
        }

        /// <summary>
        /// <see cref="IPointerClickHandler"/>-Implementation. Calls <see cref="OnClickCity"/>.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClickCity?.Invoke(this);
        }
        
        /// <summary>
        /// Visually selects CityObject.
        /// </summary>
        public void Select()
        {
            img.color = selectedColor;
        }

        /// <summary>
        /// Visually deselects CityObject.
        /// </summary>
        public void Deselect()
        {
            if (img != null)
                img.color = defaultColor;
        }

        /// <summary>
        /// Initializes internal references.
        /// </summary>
        private void Awake()
        {
            img = GetComponent<Image>();
            defaultColor = img.color;
        }
    }
}
