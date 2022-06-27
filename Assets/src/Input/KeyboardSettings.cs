using Game.UI;
using Game.Utils;
using UnityEngine;

namespace Game.Input
{
    public class KeyboardSettings : MonoBehaviour
    {
        public static KeyboardSettings Instance;

        /// <summary>
        /// Initializiation
        /// </summary>
        private void Start()
        {
            if (Instance != null) {
                CustomLogger.Error("{AttemptingToCreateMultipleInstances}");
                return;
            }
            Instance = this;

            //TODO: Load from file

            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.Escape, () => { UIManager.Instance.HandleWindowEventKeydown(WindowEvent.Close); }, KeyEventTag.IgnoreUI);
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.Return, () => { UIManager.Instance.HandleWindowEventKeydown(WindowEvent.Accept); }, KeyEventTag.IgnoreUI);
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.F12, () => { ConsoleManager.Instance.Active = !ConsoleManager.Instance.Active; }, KeyEventTag.IgnoreUI);
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.UpArrow, () => { ConsoleManager.Instance.HistoryUp(); }, KeyEventTag.IgnoreUI);
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.DownArrow, () => { ConsoleManager.Instance.HistoryDown(); }, KeyEventTag.IgnoreUI);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        { }
    }
}
