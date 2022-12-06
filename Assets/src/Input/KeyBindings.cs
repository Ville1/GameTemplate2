using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Input
{
    public class KeyBindings
    {
        private static List<KeyBinding> bindings = null;

        public static List<KeyBinding> All
        {
            get {
                Initialize();
                return bindings;
            }
        }

        public static KeyBinding Get(string internalName)
        {
            Initialize();
            KeyBinding binding = bindings.FirstOrDefault(b => b.InternalName == internalName);
            if(binding == null) {
                CustomLogger.Error("KeyBindingNotFound", internalName);
                return null;
            }
            return binding;
        }

        public static KeyBinding CloseWindow { get { return Get("CloseWindow"); } }
        public static KeyBinding AcceptWindow { get { return Get("AcceptWindow"); } }
        public static KeyBinding Wave { get { return Get("Wave"); } }

        private static void Initialize()
        {
            if(bindings != null) {
                return;
            }
            bindings = new List<KeyBinding>();

            bindings.Add(KeyBinding.Load("CloseWindow", "{KeyBindingCloseWindow}", KeyCode.Escape));
            bindings.Add(KeyBinding.Load("AcceptWindow", "{KeyBindingAccept}", KeyCode.Return));
            bindings.Add(KeyBinding.Load("Wave", "Wave", KeyCode.Alpha1));
        }
    }

    public class KeyBinding
    {
        public string InternalName { get; private set; }
        public LString Name { get; private set; }
        public KeyCode KeyCode { get; set; }
        public Guid? EventListenerId { get; set; }
        public KeyboardManager.KeyEventType? EventListenerType { get; set; }

        public KeyBinding(string internalName, LString name, KeyCode keyCode)
        {
            InternalName = internalName;
            Name = name;
            KeyCode = keyCode;
            EventListenerId = null;
        }

        public KeyBinding(KeyBinding keyBinding)
        {
            InternalName = keyBinding.InternalName;
            Name = keyBinding.Name;
            KeyCode = keyBinding.KeyCode;
            EventListenerId = null;
        }

        public void Rebind(KeyCode keyCode)
        {
            KeyCode oldKeyCode = KeyCode;
            KeyCode = keyCode;
            if (!EventListenerId.HasValue || !EventListenerType.HasValue) {
                CustomLogger.Error("{KeyBindingNotRegistered}", InternalName);
            } else {
                KeyboardManager.Instance.Rebind(EventListenerType.Value, EventListenerId.Value, oldKeyCode, KeyCode);
            }
        }

        public static KeyBinding Load(string internalName, LString name, KeyCode defaultKeyCode)
        {
            //TODO: Load from file (if file exists)
            return new KeyBinding(internalName, name, defaultKeyCode);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", InternalName, KeyCode);
        }
    }
}
