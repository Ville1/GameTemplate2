using Game.UI;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Input
{
    public class KeyboardManager : MonoBehaviour
    {
        public enum KeyEventType { Down, Up, Held }

        public delegate void OnKeyDownDelegate();

        public static KeyboardManager Instance;

        private KeyEventListenerContainer keyDownEvents = new KeyEventListenerContainer(KeyEventType.Down);
        private KeyEventListenerContainer keyUpEvents = new KeyEventListenerContainer(KeyEventType.Up);
        private KeyEventListenerContainer keyHeldEvents = new KeyEventListenerContainer(KeyEventType.Held);

        private AnyKeyEventListenerContainer anyKeyDownEvents = new AnyKeyEventListenerContainer(KeyEventType.Down);
        private AnyKeyEventListenerContainer anyKeyUpEvents = new AnyKeyEventListenerContainer(KeyEventType.Up);
        private AnyKeyEventListenerContainer anyKeyHeldEvents = new AnyKeyEventListenerContainer(KeyEventType.Held);

        protected bool running = false;

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
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {
            running = true;

            //If events are added or removed then game is running, operations need to be added to a queue to avoid this:
            //InvalidOperationException: Collection was modified; enumeration operation may not execute.

            bool inputFieldIsFocussed = EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null;

            keyDownEvents.Activate(inputFieldIsFocussed);
            keyUpEvents.Activate(inputFieldIsFocussed);
            keyHeldEvents.Activate(inputFieldIsFocussed);

            anyKeyDownEvents.Activate(inputFieldIsFocussed);
            anyKeyUpEvents.Activate(inputFieldIsFocussed);
            anyKeyHeldEvents.Activate(inputFieldIsFocussed);
        }

        //##### Add event listeners #####
        //----- Key down -----
        public Guid AddOnKeyDownEventListener(KeyCode key, OnKeyDownDelegate onKeyDown, KeyEventTag tag)
        {
            return AddOnKeyDownEventListener(key, onKeyDown, 0, new List<KeyEventTag>() { tag });
        }

        public Guid AddOnKeyDownEventListener(KeyCode key, OnKeyDownDelegate onKeyDown, int priority = 0, List<KeyEventTag> tags = null)
        {
            return AddEventListener(keyDownEvents, key, onKeyDown, priority, tags);
        }

        public Guid AddOnKeyDownEventListener(KeyBinding keyBinding, OnKeyDownDelegate onKeyDown, KeyEventTag tag)
        {
            return AddOnKeyDownEventListener(keyBinding, onKeyDown, 0, new List<KeyEventTag>() { tag });
        }

        public Guid AddOnKeyDownEventListener(KeyBinding keyBinding, OnKeyDownDelegate onKeyDown, int priority = 0, List<KeyEventTag> tags = null)
        {
            return AddEventListener(keyDownEvents, KeyEventType.Down, keyBinding, onKeyDown, priority, tags);
        }

        //----- Key up -----
        public Guid AddOnKeyUpEventListener(KeyCode key, OnKeyDownDelegate onKeyUp, KeyEventTag tag)
        {
            return AddOnKeyUpEventListener(key, onKeyUp, 0, new List<KeyEventTag>() { tag });
        }

        public Guid AddOnKeyUpEventListener(KeyCode key, OnKeyDownDelegate onKeyUp, int priority = 0, List<KeyEventTag> tags = null)
        {
            return AddEventListener(keyUpEvents, key, onKeyUp, priority, tags);
        }

        public Guid AddOnKeyUpEventListener(KeyBinding keyBinding, OnKeyDownDelegate onKeyUp, KeyEventTag tag)
        {
            return AddOnKeyUpEventListener(keyBinding, onKeyUp, 0, new List<KeyEventTag>() { tag });
        }

        public Guid AddOnKeyUpEventListener(KeyBinding keyBinding, OnKeyDownDelegate onKeyUp, int priority = 0, List<KeyEventTag> tags = null)
        {
            return AddEventListener(keyUpEvents, KeyEventType.Up, keyBinding, onKeyUp, priority, tags);
        }

        //----- Key held -----
        public Guid AddKeyHeldEventListener(KeyCode key, OnKeyDownDelegate onKeyHeld, KeyEventTag tag)
        {
            return AddKeyHeldEventListener(key, onKeyHeld, 0, new List<KeyEventTag>() { tag });
        }

        public Guid AddKeyHeldEventListener(KeyCode key, OnKeyDownDelegate onKeyHeld, int priority = 0, List<KeyEventTag> tags = null)
        {
            return AddEventListener(keyHeldEvents, key, onKeyHeld, priority, tags);
        }

        public Guid AddKeyHeldEventListener(KeyBinding keyBinding, OnKeyDownDelegate onKeyHeld, KeyEventTag tag)
        {
            return AddKeyHeldEventListener(keyBinding, onKeyHeld, 0, new List<KeyEventTag>() { tag });
        }

        public Guid AddKeyHeldEventListener(KeyBinding keyBinding, OnKeyDownDelegate onKeyHeld, int priority = 0, List<KeyEventTag> tags = null)
        {
            return AddEventListener(keyHeldEvents, KeyEventType.Held, keyBinding, onKeyHeld, priority, tags);
        }

        //##### Remove event listeners #####
        //----- Key down -----
        /// <summary>
        /// Remove key down listeners from key
        /// </summary>
        /// <param name="tags">If not null, remove only events which have a tag in this list</param>
        public void RemoveOnKeyDownEventListeners(KeyCode key, List<KeyEventTag> tags = null)
        {
            RemoveEventListeners(keyDownEvents, key, tags);
        }

        /// <summary>
        /// Remove key down listeners from key with a specified tag
        /// </summary>
        public void RemoveOnKeyDownEventListeners(KeyCode key, KeyEventTag tag)
        {
            RemoveEventListeners(keyDownEvents, key, new List<KeyEventTag>() { tag });
        }

        /// <summary>
        /// Remove key down listeners from key with id
        /// </summary>
        public void RemoveOnKeyDownEventListeners(KeyCode key, Guid eventId)
        {
            RemoveEventListeners(keyDownEvents, key, eventId);
        }

        //----- Key up -----
        /// <summary>
        /// Remove key up listeners from key
        /// </summary>
        /// <param name="tags">If not null, remove only events which have a tag in this list</param>
        public void RemoveOnKeyUpEventListeners(KeyCode key, List<KeyEventTag> tags = null)
        {
            RemoveEventListeners(keyUpEvents, key, tags);
        }

        /// <summary>
        /// Remove key up listeners from key with a specified tag
        /// </summary>
        public void RemoveOnKeyUpEventListeners(KeyCode key, KeyEventTag tag)
        {
            RemoveEventListeners(keyUpEvents, key, new List<KeyEventTag>() { tag });
        }

        /// <summary>
        /// Remove key up listeners from key with id
        /// </summary>
        public void RemoveOnKeyUpEventListeners(KeyCode key, Guid eventId)
        {
            RemoveEventListeners(keyUpEvents, key, eventId);
        }

        //----- Key held -----
        /// <summary>
        /// Remove key held listeners from key
        /// </summary>
        /// <param name="tags">If not null, remove only events which have a tag in this list</param>
        public void RemoveOnKeyHeldEventListeners(KeyCode key, List<KeyEventTag> tags = null)
        {
            RemoveEventListeners(keyHeldEvents, key, tags);
        }

        /// <summary>
        /// Remove key held listeners from key with a specified tag
        /// </summary>

        public void RemoveOnKeyHeldEventListeners(KeyCode key, KeyEventTag tag)
        {
            RemoveEventListeners(keyHeldEvents, key, new List<KeyEventTag>() { tag });
        }

        /// <summary>
        /// Remove key up listeners from key with id
        /// </summary>
        public void RemoveOnKeyHeldEventListeners(KeyCode key, Guid eventId)
        {
            RemoveEventListeners(keyHeldEvents, key, eventId);
        }

        //----- Rebinding keys -----
        public void Rebind(KeyEventType type, Guid eventListenerId, KeyCode oldKeyCode, KeyCode newKeyCode)
        {
            switch (type) {
                case KeyEventType.Down:
                    keyDownEvents.Rebind(oldKeyCode, newKeyCode, eventListenerId);
                    break;
                case KeyEventType.Up:
                    keyUpEvents.Rebind(oldKeyCode, newKeyCode, eventListenerId);
                    break;
                case KeyEventType.Held:
                    keyHeldEvents.Rebind(oldKeyCode, newKeyCode, eventListenerId);
                    break;
            }
        }

        //##### Any key listeners #####
        //----- Any key down -----
        public Guid AddOnKeyDownEventListener(OnKeyDownDelegate onKeyDown, KeyEventTag tag)
        {
            return AddOnKeyDownEventListener(onKeyDown, 0, new List<KeyEventTag>() { tag });
        }

        public Guid AddOnKeyDownEventListener(OnKeyDownDelegate onKeyDown, int priority = 0, List<KeyEventTag> tags = null)
        {
            return AddEventListener(anyKeyDownEvents, onKeyDown, priority, tags);
        }

        //----- Any key up -----
        public Guid AddOnKeyUpEventListener(OnKeyDownDelegate onKeyUp, KeyEventTag tag)
        {
            return AddOnKeyUpEventListener(onKeyUp, 0, new List<KeyEventTag>() { tag });
        }

        public Guid AddOnKeyUpEventListener(OnKeyDownDelegate onKeyUp, int priority = 0, List<KeyEventTag> tags = null)
        {
            return AddEventListener(anyKeyUpEvents, onKeyUp, priority, tags);
        }

        //----- Any key held -----
        public Guid AddOnKeyHeldEventListener(OnKeyDownDelegate onKeyHeld, KeyEventTag tag)
        {
            return AddOnKeyHeldEventListener(onKeyHeld, 0, new List<KeyEventTag>() { tag });
        }

        public Guid AddOnKeyHeldEventListener(OnKeyDownDelegate onKeyHeld, int priority = 0, List<KeyEventTag> tags = null)
        {
            return AddEventListener(anyKeyHeldEvents, onKeyHeld, priority, tags);
        }

        //##### Remove any key event listeners #####
        public void RemoveOnKeyDownEventListeners(Guid id)
        {
            RemoveEventListener(anyKeyDownEvents, id);
        }

        public void RemoveOnKeyUpEventListeners(Guid id)
        {
            RemoveEventListener(anyKeyUpEvents, id);
        }

        public void RemoveOnKeyHeldEventListeners(Guid id)
        {
            RemoveEventListener(anyKeyHeldEvents, id);
        }

        private Guid AddEventListener(KeyEventListenerContainer listeners, KeyCode key, OnKeyDownDelegate onKeyDown, int priority = 0, List<KeyEventTag> tags = null)
        {
            KeyEvent keyEvent = new KeyEvent(onKeyDown, priority, tags);
            listeners.Add(key, keyEvent);
            return keyEvent.Id;
        }

        private Guid AddEventListener(KeyEventListenerContainer listeners, KeyEventType type, KeyBinding keyBinding, OnKeyDownDelegate onKeyDown, int priority = 0, List<KeyEventTag> tags = null)
        {
            if (keyBinding.EventListenerId.HasValue) {
                CustomLogger.Warning("{KeyBindingAlreadyRegistered}", keyBinding.InternalName);
                return Guid.Empty;
            }
            keyBinding.EventListenerId = AddEventListener(listeners, keyBinding.KeyCode, onKeyDown, priority, tags);
            keyBinding.EventListenerType = type;
            return keyBinding.EventListenerId.Value;
        }

        private void RemoveEventListeners(KeyEventListenerContainer listeners, KeyCode key, List<KeyEventTag> tags = null)
        {
            listeners.Remove(key, tags);
        }

        private void RemoveEventListeners(KeyEventListenerContainer listeners, KeyCode key, Guid id)
        {
            listeners.Remove(key, id);
        }

        private Guid AddEventListener(AnyKeyEventListenerContainer listeners, OnKeyDownDelegate eventDelegate, int priority = 0, List<KeyEventTag> tags = null)
        {
            KeyEvent keyEvent = new KeyEvent(eventDelegate, priority, tags);
            listeners.Add(keyEvent);
            return keyEvent.Id;
        }

        private void RemoveEventListener(AnyKeyEventListenerContainer listeners, Guid id)
        {
            listeners.Remove(id);
        }

        private class KeyEventListenerContainer
        {
            public KeyEventType KeyEvent { get; private set; }
            public Dictionary<KeyCode, KeyEventListener> Listeners { get; private set; }
            public Dictionary<KeyCode, List<KeyEvent>> AddQueue { get; private set; }
            public Dictionary<KeyCode, List<List<KeyEventTag>>> RemoveQueueTags { get; private set; }
            public Dictionary<KeyCode, List<Guid>> RemoveQueueIds { get; private set; }

            public KeyEventListenerContainer(KeyEventType keyEvent)
            {
                KeyEvent = keyEvent;
                Listeners = new Dictionary<KeyCode, KeyEventListener>();
                AddQueue = new Dictionary<KeyCode, List<KeyEvent>>();
                RemoveQueueTags = new Dictionary<KeyCode, List<List<KeyEventTag>>>();
                RemoveQueueIds = new Dictionary<KeyCode, List<Guid>>();
            }

            public void Add(KeyCode key, KeyEvent keyEvent)
            {
                if (Instance.running) {
                    if (AddQueue.ContainsKey(key)) {
                        AddQueue[key].Add(keyEvent);
                    } else {
                        AddQueue.Add(key, new List<KeyEvent>() { keyEvent });
                    }
                } else {
                    if (!Listeners.ContainsKey(key)) {
                        Listeners.Add(key, new KeyEventListener());
                    }
                    Listeners[key].Add(keyEvent);
                }
            }

            public void Remove(KeyCode key, List<KeyEventTag> tags = null)
            {
                if (Instance.running) {
                    if (RemoveQueueTags.ContainsKey(key)) {
                        RemoveQueueTags[key].Add(tags);
                    } else {
                        RemoveQueueTags.Add(key, new List<List<KeyEventTag>>() { tags });
                    }
                } else {
                    if (Listeners.ContainsKey(key)) {
                        Listeners[key].Remove(tags);
                    }
                }
            }

            public void Remove(KeyCode key, Guid id)
            {
                if (Instance.running) {
                    if (RemoveQueueIds.ContainsKey(key)) {
                        RemoveQueueIds[key].Add(id);
                    } else {
                        RemoveQueueIds.Add(key, new List<Guid>() { id });
                    }
                } else {
                    if (Listeners.ContainsKey(key)) {
                        Listeners[key].Remove(id);
                    }
                }
            }

            public void Rebind(KeyCode oldKey, KeyCode newKey, Guid id)
            {
                KeyEvent keyEvent = Listeners[oldKey].Get(id);
                Remove(oldKey, id);
                Add(newKey, keyEvent);
            }

            public void Activate(bool inputFieldIsFocussed)
            {
                //Process queues
                //Remove events by tag list
                foreach(KeyValuePair<KeyCode, List<List<KeyEventTag>>> removeWithTags in RemoveQueueTags) {
                    if (Listeners.ContainsKey(removeWithTags.Key) && Listeners[removeWithTags.Key].Events.Count != 0) {
                        foreach(List<KeyEventTag> tagList in removeWithTags.Value) {
                            Listeners[removeWithTags.Key].Remove(tagList);
                            if(tagList == null) {
                                //All listeners cleared, no reason to keep looping, since we can't remove anything anymore
                                break;
                            }
                        }
                    }
                }
                RemoveQueueTags.Clear();

                //Remove events by id
                foreach (KeyValuePair<KeyCode, List<Guid>> removeWithIds in RemoveQueueIds) {
                    if (Listeners.ContainsKey(removeWithIds.Key) && Listeners[removeWithIds.Key].Events.Count != 0) {
                        foreach (Guid id in removeWithIds.Value) {
                            Listeners[removeWithIds.Key].Remove(id);
                        }
                    }
                }
                RemoveQueueIds.Clear();

                //Add events
                foreach(KeyValuePair<KeyCode, List<KeyEvent>> newKeyEvents in AddQueue) {
                    if (!Listeners.ContainsKey(newKeyEvents.Key)) {
                        Listeners.Add(newKeyEvents.Key, new KeyEventListener());
                    }
                    foreach(KeyEvent keyEvent in newKeyEvents.Value) {
                        Listeners[newKeyEvents.Key].Add(keyEvent);
                    }
                }
                AddQueue.Clear();

                //Activate events
                foreach (KeyValuePair<KeyCode, KeyEventListener> keyListener in Listeners) {
                    bool active = false;
                    switch (KeyEvent) {
                        case KeyEventType.Down:
                            active = UnityEngine.Input.GetKeyDown(keyListener.Key);
                            break;
                        case KeyEventType.Up:
                            active = UnityEngine.Input.GetKeyUp(keyListener.Key);
                            break;
                        case KeyEventType.Held:
                            active = UnityEngine.Input.GetKey(keyListener.Key);
                            break;
                        default:
                            throw new NotImplementedException(KeyEvent.ToString());
                    }
                    if (active) {
                        keyListener.Value.Activate(inputFieldIsFocussed);
                    }
                }
            }
        }

        private class KeyEventListener
        {
            public List<KeyEvent> Events { get; set; } = new List<KeyEvent>();

            public void Activate(bool inputFieldIsFocussed)
            {
                foreach (KeyEvent keyEvent in Events) {
                    if(UIManager.Instance.CanFire(keyEvent.Tags, inputFieldIsFocussed)) {
                        keyEvent.Listener();
                    }
                }
            }

            public bool Has(Guid eventId)
            {
                return Events.Any(keyEvent => keyEvent.Id == eventId);
            }

            public KeyEvent Get(Guid eventId)
            {
                return Events.FirstOrDefault(keyEvent => keyEvent.Id == eventId);
            }

            public void Add(KeyEvent keyEvent)
            {
                Events.Add(keyEvent);
                Events = Events.OrderByDescending(keyEvent => keyEvent.Priority).ToList();
            }

            public void Remove(List<KeyEventTag> tags = null)
            {
                if (tags == null) {
                    //Remove all
                    Events.Clear();
                } else {
                    //Remove events with tags
                    Events = Events.Where(oldEvent => !oldEvent.Tags.Any(tag => tags.Contains(tag))).OrderByDescending(keyEvent => keyEvent.Priority).ToList();
                }
            }

            public bool Remove(Guid eventId)
            {
                if (Has(eventId)) {
                    Events = Events.Where(keyEvent => keyEvent.Id != eventId).OrderByDescending(keyEvent => keyEvent.Priority).ToList();
                    return true;
                }
                return false;
            }
        }

        private class AnyKeyEventListenerContainer
        {
            public KeyEventType KeyEvent { get; private set; }
            public KeyEventListener Listener { get; private set; }
            public List<KeyEvent> AddQueue { get; private set; }
            public List<Guid> RemoveQueueIds { get; private set; }

            public AnyKeyEventListenerContainer(KeyEventType keyEvent)
            {
                KeyEvent = keyEvent;
                Listener = new KeyEventListener();
                AddQueue = new List<KeyEvent>();
                RemoveQueueIds = new List<Guid>();
            }

            public void Add(KeyEvent keyEvent)
            {
                if (Instance.running) {
                    AddQueue.Add(keyEvent);
                } else {
                    Listener.Add(keyEvent);
                }
            }

            public void Remove(Guid id)
            {
                if (Instance.running) {
                    RemoveQueueIds.Add(id);
                } else {
                    Listener.Remove(id);
                }
            }

            public void Activate(bool inputFieldIsFocussed)
            {
                //Process queues

                //Remove events by id
                foreach (Guid id in RemoveQueueIds) {
                    Listener.Remove(id);
                }
                RemoveQueueIds.Clear();

                //Add events
                foreach (KeyEvent keyEvent in AddQueue) {
                    Listener.Add(keyEvent);
                }
                AddQueue.Clear();

                //Activate events
                bool active = false;
                foreach(KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
                    switch (KeyEvent) {
                        case KeyEventType.Down:
                            active = UnityEngine.Input.GetKeyDown(keyCode);
                            break;
                        case KeyEventType.Up:
                            active = UnityEngine.Input.GetKeyUp(keyCode);
                            break;
                        case KeyEventType.Held:
                            active = UnityEngine.Input.GetKey(keyCode);
                            break;
                        default:
                            throw new NotImplementedException(KeyEvent.ToString());
                    }
                    if (active) {
                        break;
                    }
                }
                if (active) {
                    Listener.Activate(inputFieldIsFocussed);
                }
            }
        }

        private class KeyEvent
        {
            public Guid Id { get; set; }
            public OnKeyDownDelegate Listener { get; set; }
            public int Priority { get; set; }
            public List<KeyEventTag> Tags { get; set; }

            public KeyEvent(OnKeyDownDelegate listener, int priority, List<KeyEventTag> tags)
            {
                Id = Guid.NewGuid();
                Listener = listener;
                Priority = priority;
                Tags = tags ?? new List<KeyEventTag>();
            }
        }
    }
}
