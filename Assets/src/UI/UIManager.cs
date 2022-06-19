using Game.Input;
using Game.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.UI {
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        /// <summary>
        /// List of all windows in the game. This should only be used to access windows, and should not be manipulated outside WindowBase.Start().
        /// </summary>
        public List<WindowBase> Windows { get; private set; } = new List<WindowBase>();

        /// <summary>
        /// Initializiation
        /// </summary>
        private void Start()
        {
            if (Instance != null) {
                CustomLogger.Error("AttemptingToCreateMultipleInstances");
                return;
            }
            Instance = this;

            MouseManager.Instance.AddEventListerener(MouseButton.Left, new MouseNothingClickEvent(CloseAllWindows));
            MouseManager.Instance.AddEventListerener(MouseButton.Middle, new MouseNothingClickEvent(CloseAllWindows));
            MouseManager.Instance.AddEventListerener(MouseButton.Right, new MouseNothingClickEvent(CloseAllWindows));
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        { }

        public void CloseAllWindows()
        {
            CloseWindows(null, null);
        }

        public void CloseWindows(WindowBase.Tag withTag)
        {
            CloseWindows(new List<WindowBase.Tag>() { withTag }, null);
        }

        public void CloseWindows(List<WindowBase.Tag> include)
        {
            CloseWindows(include, null);
        }

        public void CloseWindows(List<WindowBase.Tag> include, WindowBase.Tag exclude)
        {
            CloseWindows(include, new List<WindowBase.Tag>() { exclude });
        }

        public void CloseWindows(List<WindowBase.Tag> include, List<WindowBase.Tag> exclude)
        {
            include = include ?? new List<WindowBase.Tag>();
            exclude = exclude ?? new List<WindowBase.Tag>();
            foreach (WindowBase window in Windows) {
                if((Main.Instance.State != State.MainMenu || !window.Tags.Contains(WindowBase.Tag.MainMenu)) &&
                    (include.Count == 0 || window.Tags.Any(tag => include.Contains(tag))) &&
                    (exclude.Count == 0 || !window.Tags.Any(tag => exclude.Contains(tag)))) {
                    window.Active = false;
                }
            }
        }

        public void HandleWindowEventKeydown(WindowEvent windowEvent)
        {
            foreach(WindowBase window in Windows) {
                //TODO: Im not completely sure about this
                //Maybe this if could be simplified to be only:
                //window.Active && window.HandleWindowEvent(windowEvent)
                if ((CanFire(new List<KeyEventTag>()) || (window.Active && window.BlockKeyboardInputs)) && window.HandleWindowEvent(windowEvent)) {
                    break;
                }
            }
        }

        public bool CanFire(List<KeyEventTag> tags)
        {
            return !Windows.Any(window => window.Active && window.BlockKeyboardInputs && !window.AllowedKeyEvents.Any(allowed => tags.Contains(allowed)));
        }

        public bool CanFire(List<MouseEventTag> tags)
        {
            return !Windows.Any(window => window.Active && window.BlockMouseEvents && !window.AllowedMouseEvents.Any(allowed => tags.Contains(allowed)));
        }
    }
}