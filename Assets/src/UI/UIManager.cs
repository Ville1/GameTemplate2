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
        public bool KeyboardInputsBlocked { get { return Windows.Any(window => window.Active && window.BlockKeyboardInputs); } }

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
            foreach (WindowBase windows in Windows) {
                if((include.Count == 0 || windows.Tags.Any(tag => include.Contains(tag))) && (exclude.Count == 0 || !windows.Tags.Any(tag => exclude.Contains(tag))))
                windows.Active = false;
            }
        }

        public void HandleWindowEventKeydown(WindowEvent windowEvent)
        {
            foreach(WindowBase window in Windows) {
                if(!KeyboardInputsBlocked || (window.Active && window.BlockKeyboardInputs)) {
                    window.HandleWindowEvent(windowEvent);
                }
            }
        }
    }
}