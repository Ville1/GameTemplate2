using UnityEngine;

namespace Game.UI
{
    public class PanelBase : MonoBehaviour
    {
        public GameObject Panel;

        /// <summary>
        /// Initializiation
        /// </summary>
        protected virtual void Start()
        { }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected virtual void Update()
        { }

        public bool Active
        {
            get {
                return Panel.activeSelf;
            }
            set {
                Panel.SetActive(value);
            }
        }

        public RectTransform RectTransform
        {
            get {
                return Panel.GetComponent<RectTransform>();
            }
        }

        public float Width
        {
            get {
                return RectTransform.rect.width;
            }
            set {
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value);
            }
        }

        public float Height
        {
            get {
                return RectTransform.rect.height;
            }
            set {
                RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
            }
        }
    }
}