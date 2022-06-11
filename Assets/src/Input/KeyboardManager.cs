using Game.UI;
using Game.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Input
{
    public class KeyboardManager : MonoBehaviour
    {
        public delegate void OnKeyDownDelegate();

        public static KeyboardManager Instance;

        private Dictionary<KeyCode, KeyEventListener> keyDownEvents = new Dictionary<KeyCode, KeyEventListener>();
        private Dictionary<KeyCode, KeyEventListener> keyUpEvents = new Dictionary<KeyCode, KeyEventListener>();
        private Dictionary<KeyCode, KeyEventListener> keyHeldEvents = new Dictionary<KeyCode, KeyEventListener>();

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
        {
            foreach (KeyValuePair<KeyCode, KeyEventListener> keyEvents in keyDownEvents) {
                if (UnityEngine.Input.GetKeyDown(keyEvents.Key)) {
                    keyEvents.Value.Activate();
                }
            }
            foreach (KeyValuePair<KeyCode, KeyEventListener> keyEvents in keyUpEvents) {
                if (UnityEngine.Input.GetKeyUp(keyEvents.Key)) {
                    keyEvents.Value.Activate();
                }
            }
            foreach (KeyValuePair<KeyCode, KeyEventListener> keyEvents in keyHeldEvents) {
                if (UnityEngine.Input.GetKey(keyEvents.Key)) {
                    keyEvents.Value.Activate();
                }
            }
        }

        public void AddOnKeyDownEventListener(KeyCode key, OnKeyDownDelegate onKeyDown, KeyEventTag tag)
        {
            AddOnKeyDownEventListener(key, onKeyDown, 0, new List<KeyEventTag>() { tag });
        }

        public void AddOnKeyDownEventListener(KeyCode key, OnKeyDownDelegate onKeyDown, int priority = 0, List<KeyEventTag> tags = null)
        {
            if (!keyDownEvents.ContainsKey(key)) {
                keyDownEvents.Add(key, new KeyEventListener());
            }
            keyDownEvents[key].Add(new KeyEvent(onKeyDown, priority, tags));
        }

        public void AddOnKeyUpEventListener(KeyCode key, OnKeyDownDelegate onKeyUp, KeyEventTag tag)
        {
            AddOnKeyUpEventListener(key, onKeyUp, 0, new List<KeyEventTag>() { tag });
        }

        public void AddOnKeyUpEventListener(KeyCode key, OnKeyDownDelegate onKeyUp, int priority = 0, List<KeyEventTag> tags = null)
        {
            if (!keyDownEvents.ContainsKey(key)) {
                keyUpEvents.Add(key, new KeyEventListener());
            }
            keyUpEvents[key].Add(new KeyEvent(onKeyUp, priority, tags));
        }

        public void AddKeyHeldEventListener(KeyCode key, OnKeyDownDelegate onKeyHeld, KeyEventTag tag)
        {
            AddKeyHeldEventListener(key, onKeyHeld, 0, new List<KeyEventTag>() { tag });
        }

        public void AddKeyHeldEventListener(KeyCode key, OnKeyDownDelegate onKeyHeld, int priority = 0, List<KeyEventTag> tags = null)
        {
            if (!keyDownEvents.ContainsKey(key)) {
                keyHeldEvents.Add(key, new KeyEventListener());
            }
            keyHeldEvents[key].Add(new KeyEvent(onKeyHeld, priority, tags));
        }

        public void RemoveEventListener()
        {

        }

        private class KeyEventListener
        {
            public List<KeyEvent> Events { get; set; } = new List<KeyEvent>();

            public void Activate()
            {
                foreach (KeyEvent keyEvent in Events) {
                    if(keyEvent.Tags.Contains(KeyEventTag.UI) || !UIManager.Instance.KeyboardInputsBlocked) {
                        keyEvent.Listener();
                    }
                }
            }

            public void Add(KeyEvent keyEvent)
            {
                Events.Add(keyEvent);
                Events = Events.OrderByDescending(keyEvent => keyEvent.Priority).ToList();
            }
        }

        private class KeyEvent
        {
            public OnKeyDownDelegate Listener { get; set; }
            public int Priority { get; set; }
            public List<KeyEventTag> Tags { get; set; }

            public KeyEvent(OnKeyDownDelegate listener, int priority, List<KeyEventTag> tags)
            {
                Listener = listener;
                Priority = priority;
                Tags = tags ?? new List<KeyEventTag>();
            }
        }
    }
}
