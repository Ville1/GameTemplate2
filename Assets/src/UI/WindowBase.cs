using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class WindowBase : MonoBehaviour
    {
        public enum Tag { ProgressBar }

        public GameObject Panel;
        public List<Tag> Tags { get; private set; } = new List<Tag>();
        public List<KeyEventTag> AllowedKeyEvents { get; private set; } = new List<KeyEventTag>() { KeyEventTag.IgnoreUI };
        public List<MouseEventTag> AllowedMouseEvents { get; private set; } = new List<MouseEventTag> { MouseEventTag.IgnoreUI };

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

        public bool BlockKeyboardInputs
        {
            get {
                return AllowedKeyEvents != null;
            }
            set {
                if (value) {
                    AllowedKeyEvents = new List<KeyEventTag>() { KeyEventTag.IgnoreUI };
                } else {
                    AllowedKeyEvents = null;
                }
            }
        }

        public bool BlockMouseEvents
        {
            get {
                return AllowedMouseEvents != null;
            }
            set {
                if (value) {
                    AllowedMouseEvents = new List<MouseEventTag>() { MouseEventTag.IgnoreUI };
                } else {
                    AllowedMouseEvents = null;
                }
            }
        }

        public virtual bool HandleWindowEvent(WindowEvent windowEvent)
        {
            return false;
        }
    }
}