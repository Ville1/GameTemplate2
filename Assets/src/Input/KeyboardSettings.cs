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

            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyBindings.CloseWindow, () => { UIManager.Instance.HandleWindowEventKeydown(WindowEvent.Close); }, KeyEventTag.IgnoreUI);
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyBindings.AcceptWindow, () => { UIManager.Instance.HandleWindowEventKeydown(WindowEvent.Accept); }, KeyEventTag.IgnoreUI);
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.F12, () => { ConsoleManager.Instance.Active = !ConsoleManager.Instance.Active; }, KeyEventTag.IgnoreUI);
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.UpArrow, () => { ConsoleManager.Instance.HistoryUp(); }, KeyEventTag.IgnoreUI);
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.DownArrow, () => { ConsoleManager.Instance.HistoryDown(); }, KeyEventTag.IgnoreUI);
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.Tab, () => { ConsoleManager.Instance.AutoComplete(); }, KeyEventTag.IgnoreUI);

            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.Keypad8, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.Move(Direction.North); } });
            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.Keypad9, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.Move(Direction.NorthEast); } });
            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.Keypad6, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.Move(Direction.East); } });
            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.Keypad3, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.Move(Direction.SouthEast); } });
            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.Keypad2, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.Move(Direction.South); } });
            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.Keypad1, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.Move(Direction.SouthWest); } });
            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.Keypad4, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.Move(Direction.West); } });
            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.Keypad7, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.Move(Direction.NorthWest); } });
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyBindings.Wave, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.Wave(); } });
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.Alpha2, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.Horn(); } });
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.Alpha3, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.Stop(); } });
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.Alpha8, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.SlowDownAnimation(); } });
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.Alpha9, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.SpeedUpAnimation(); } });
            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyCode.Alpha0, () => { if (Main.Instance.State == State.Running) { Main.Instance.PlayerCharacter.ToggleAnimationPause(); } });
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        { }
    }
}
