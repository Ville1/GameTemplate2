using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class WindowBase : MonoBehaviour
    {
        public enum Tag { ProgressBar }

        public GameObject Panel;
        public List<Tag> Tags { get; private set; } = new List<Tag>();

        /// <summary>
        /// Initializiation
        /// </summary>
        protected virtual void Start()
        {
            UIManager.Instance.Windows.Add(this);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected virtual void Update()
        { }

        public virtual bool Active
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