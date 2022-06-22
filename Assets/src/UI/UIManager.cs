using Game.Input;
using Game.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI {
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        private List<WindowBase> windows = new List<WindowBase>();

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

        /// <summary>
        /// This should not be manipulated called outside WindowBase.Start().
        /// </summary>
        public void RegisterWindow(WindowBase window)
        {
            windows.Add(window);
            windows = windows.OrderByDescending(window => window.WindowEventPriority).ToList();
        }

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
            foreach (WindowBase window in windows) {
                if(window.Active &&
                    (Main.Instance.State != State.MainMenu || !window.Tags.Contains(WindowBase.Tag.MainMenu)) &&
                    (include.Count == 0 || window.Tags.Any(tag => include.Contains(tag))) &&
                    (exclude.Count == 0 || !window.Tags.Any(tag => exclude.Contains(tag)))) {
                    window.Active = false;
                }
            }
        }

        public void HandleWindowEventKeydown(WindowEvent windowEvent)
        {
            foreach (WindowBase window in windows) {
                if ((window.Active || window.Tags.Contains(WindowBase.Tag.MainMenu)) && window.HandleWindowEvent(windowEvent)) {
                    break;
                }
            }
        }

        public bool CanFire(List<KeyEventTag> tags)
        {
            return !windows.Any(window => window.Active && window.BlockKeyboardInputs && !window.AllowedKeyEvents.Any(allowed => tags.Contains(allowed)));
        }

        public bool CanFire(MouseEventData eventData)
        {
            return (!eventData.IsBlockedByUI || !EventSystem.current.IsPointerOverGameObject()) &&
                !windows.Any(window => window.Active && window.BlockMouseEvents && !window.AllowedMouseEvents.Any(allowed => eventData.Tags.Contains(allowed)));
        }
    }
}