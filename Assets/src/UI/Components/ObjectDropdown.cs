using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace Game.UI.Components
{
    public class ObjectDropdown<ObjectType> where ObjectType : IDropdownOption
    {
        public CustomDropdown BaseDropdown { get; private set; }
        public List<ObjectType> Items { get; private set; }
        public Action<ObjectType> OnChange { get; private set; }

        public ObjectDropdown(TMP_Dropdown baseDropdown, Action<ObjectType> onChange, List<ObjectType> items = null)
        {
            Initialize(new CustomDropdown(baseDropdown, HandleChange), onChange, items);
        }

        public ObjectDropdown(TMP_Dropdown baseDropdown, Action<ObjectType> onChange, Image captionImage, Image itemImage, List<ObjectType> items = null)
        {
            Initialize(new CustomDropdown(baseDropdown, HandleChange, captionImage, itemImage), onChange, items);
        }

        public ObjectDropdown(TMP_Dropdown baseDropdown, Action<ObjectType> onChange, string captionImageName, string itemImageName, List<ObjectType> items = null)
        {
            Initialize(new CustomDropdown(baseDropdown, HandleChange, captionImageName, itemImageName), onChange, items);
        }

        public ObjectDropdown(TMP_Dropdown baseDropdown, Action<ObjectType> onChange, bool generateCaptionImage, bool generateItemImage, List<ObjectType> items = null)
        {
            Initialize(new CustomDropdown(baseDropdown, HandleChange, generateCaptionImage, generateItemImage), onChange, items);
        }

        private void Initialize(CustomDropdown baseDropdown, Action<ObjectType> onChange, List<ObjectType> items)
        {
            BaseDropdown = baseDropdown;
            OnChange = onChange;
            Items = items ?? new List<ObjectType>();
            foreach(ObjectType item in Items) {
                AddBaseOption(item);
            }
        }

        public void AddOption(ObjectType obj)
        {
            Items.Add(obj);
            AddBaseOption(obj);
        }

        private void AddBaseOption(ObjectType obj)
        {
            if (obj.DropdownSprite == null) {
                BaseDropdown.AddOption(obj.DropdownText);
            } else {
                BaseDropdown.AddOption(obj.DropdownText, obj.DropdownSprite);
            }
        }

        public void Clear()
        {
            BaseDropdown.Clear();
            Items.Clear();
        }

        public ObjectType Value
        {
            get {
                return Items.Count == 0 ? default(ObjectType) : Items[BaseDropdown.Value];
            }
            set {
                if(Items.Count != 0) {
                    BaseDropdown.Value = Items.IndexOf(value);
                }
            }
        }

        public int SelectedIndex
        {
            get {
                return BaseDropdown.Value;
            }
            set {
                BaseDropdown.Value = value;
            }
        }

        public bool Interactable
        {
            get {
                return BaseDropdown.Interactable;
            }
            set {
                BaseDropdown.Interactable = value;
            }
        }

        private void HandleChange(int index)
        {
            if (OnChange != null) {
                OnChange(Items[index]);
            }
        }
    }

    public interface IDropdownOption
    {
        LString DropdownText { get; }
        /// <summary>
        /// Can be left as null, if no sprite is needed
        /// </summary>
        SpriteData DropdownSprite { get; }
    }
}
