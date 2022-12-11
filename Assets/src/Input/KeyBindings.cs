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

        public static void SaveChanges()
        {
            Initialize();
            Utils.Config.KeyBindingsFileManager.Save(bindings);
        }

        public static KeyBinding CloseWindow { get { return Get("CloseWindow"); } }
        public static KeyBinding AcceptWindow { get { return Get("AcceptWindow"); } }
        public static KeyBinding NotificationHistory { get { return Get("NotificationHistory"); } }
        public static KeyBinding CloseAllNotifications { get { return Get("CloseAllNotifications"); } }

        public static KeyBinding Wave { get { return Get("Wave"); } }
        public static KeyBinding Horn { get { return Get("Horn"); } }
        public static KeyBinding Stop { get { return Get("Stop"); } }

        private static void Initialize()
        {
            if(bindings != null) {
                return;
            }
            bindings = new List<KeyBinding>();

            bindings.Add(KeyBinding.Load("CloseWindow", "{KeyBindingCloseWindow}", KeyCode.Escape, KeyBindingCategories.Window));
            bindings.Add(KeyBinding.Load("AcceptWindow", "{KeyBindingAccept}", KeyCode.Return, KeyBindingCategories.Window));
            bindings.Add(KeyBinding.Load("NotificationHistory", "{KeyBindingNotificationHistory}", KeyCode.N, KeyBindingCategories.Window));
            bindings.Add(KeyBinding.Load("CloseAllNotifications", "{KeyBindingCloseAllNotifications}", KeyCode.Delete, KeyBindingCategories.Misc,
                new List<KeyBindingCategory>() { KeyBindingCategories.Any }));

            bindings.Add(KeyBinding.Load("Wave", "Wave", KeyCode.Alpha1, KeyBindingCategories.Gameplay));
            bindings.Add(KeyBinding.Load("Horn", "Horn", KeyCode.Alpha2, KeyBindingCategories.Gameplay));
            bindings.Add(KeyBinding.Load("Stop", "Stop", KeyCode.Alpha3, KeyBindingCategories.Gameplay));
        }
    }

    public class KeyBinding
    {
        public string InternalName { get; private set; }
        public LString Name { get; private set; }
        public KeyCode KeyCode { get; set; }
        /// <summary>
        /// Keybindings are grouped by these in the settings window. Also used to check for conflicts.
        /// </summary>
        public KeyBindingCategory Category { get; private set; }
        /// <summary>
        /// Settings window does not allow duplicates in these categories. If null or empty list, conflicts are not checked. Use new List<KeyBindingCategory>() { KeyBindingCategories.Any }, to check all.
        /// </summary>
        public List<KeyBindingCategory> ConflictingCategories { get; private set; }
        /// <summary>
        /// Can this keybinding conflict with others?
        /// </summary>
        public bool HasConflictingCategories { get { return ConflictingCategories != null && ConflictingCategories.Count != 0; } }
        public Guid? EventListenerId { get; set; }
        public KeyboardManager.KeyEventType? EventListenerType { get; set; }

        public KeyBinding(string internalName, LString name, KeyCode keyCode, KeyBindingCategory category, List<KeyBindingCategory> conflictingCategories = null)
        {
            InternalName = internalName;
            Name = name;
            KeyCode = keyCode;
            Category = category;
            //If conflictingCategories has KeyBindingCategories.Any, it does not need to have any other categories in it, as Any already matches them all 
            ConflictingCategories = conflictingCategories != null ? (conflictingCategories.Contains(KeyBindingCategories.Any) ?
                new List<KeyBindingCategory>() { KeyBindingCategories.Any } : conflictingCategories) : new List<KeyBindingCategory>() { category };
            EventListenerId = null;
            EventListenerType = null;
        }

        public KeyBinding(KeyBinding keyBinding)
        {
            InternalName = keyBinding.InternalName;
            Name = keyBinding.Name;
            KeyCode = keyBinding.KeyCode;
            Category = keyBinding.Category;
            ConflictingCategories = keyBinding.ConflictingCategories.Select(category => category).ToList();
            EventListenerId = null;
            EventListenerType = null;
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

        public static KeyBinding Load(string internalName, LString name, KeyCode defaultKeyCode, KeyBindingCategory category, List<KeyBindingCategory> conflictingCategories = null)
        {
            KeyBinding keyBinding = new KeyBinding(internalName, name, defaultKeyCode, category, conflictingCategories);
            Utils.Config.KeyBindingsFileManager.Load(keyBinding);
            return keyBinding;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", InternalName, KeyCode);
        }
    }

    public class KeyBindingCategory
    {
        /// <summary>
        /// Order in UI, ascending. If < 0, this is not shown in UI.
        /// </summary>
        public int Order { get; private set; }
        public string InternalName { get; private set; }
        public LString Name { get; private set; }
        public bool IsInternal { get { return Order < 0; } }

        public KeyBindingCategory(int order, string internalName, LString name)
        {
            Order = order;
            InternalName = internalName;
            Name = name;
        }
    }

    public class KeyBindingCategories
    {
        private static List<KeyBindingCategory> categories = null;

        public static KeyBindingCategory Any { get { return Get("Any"); } }
        public static KeyBindingCategory Window { get { return Get("Windows"); } }
        public static KeyBindingCategory Gameplay { get { return Get("Gameplay"); } }
        public static KeyBindingCategory Misc { get { return Get("Misc"); } }

        public static List<KeyBindingCategory> All { get { Initialize(); return categories; } }

        private static void Initialize()
        {
            if(categories != null) {
                return;
            }
            categories = new List<KeyBindingCategory>() {
                new KeyBindingCategory(-1, "Any", null),
                new KeyBindingCategory(0, "Windows", "{Windows}"),
                new KeyBindingCategory(1, "Gameplay", "{Gameplay}"),
                new KeyBindingCategory(2, "Misc", "{Misc}")
            };
        }

        private static KeyBindingCategory Get(string internalName)
        {
            Initialize();
            return categories.FirstOrDefault(catogory => catogory.InternalName == internalName);
        }
    }
}
