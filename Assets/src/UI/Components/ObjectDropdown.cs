using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

namespace Game.UI.Components
{
    /// <summary>
    /// </summary>
    /// <typeparam name="ObjectType">Note: It is assumed that default(ObjectType) is null</typeparam>
    public class ObjectDropdown<ObjectType> where ObjectType : IDropdownOption
    {
        public CustomDropdown BaseDropdown { get; private set; }
        public List<ObjectType> Items { get; private set; }
        public Action<ObjectType> OnChange { get; private set; }
        public bool HasNoneOption { get { return noneOption != null; } }
        /// <summary>
        /// If true, option matching null value will be hidden, if not selected
        /// </summary>
        public bool HideNoneOption { get; set; }

        private TMP_Dropdown.OptionData noneOption;
        private bool noneOptionIsVisible;

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
            if (Items.Contains(default(ObjectType))) {
                throw new ArgumentException("Null values should be added with AddNoneOption");
            }
            noneOption = null;
            HideNoneOption = false;
            noneOptionIsVisible = false;
            foreach (ObjectType item in Items) {
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

        /// <summary>
        /// Add an option for a null value. Note: if one is already added, it will be overwriten.
        /// </summary>
        public void AddNoneOption(LString text, IHasSprite objectWithSprite = null)
        {
            AddNoneOption(new TMP_Dropdown.OptionData() {
                text = text,
                image = objectWithSprite == null ? null : TextureManager.GetSprite(objectWithSprite)
            });
        }

        /// <summary>
        /// Add an option for a null value. Note: if one is already added, it will be overwriten.
        /// </summary>
        public void AddNoneOption(LString text, SpriteData spriteData)
        {
            AddNoneOption(new TMP_Dropdown.OptionData() {
                text = text,
                image = spriteData == null || spriteData.IsEmpty ? null : TextureManager.GetSprite(spriteData)
            });
        }

        /// <summary>
        /// Add an option for a null value. Note: if one is already added, it will be overwriten.
        /// </summary>
        public void AddNoneOption(LString text, string spriteName)
        {
            AddNoneOption(new TMP_Dropdown.OptionData() {
                text = text,
                image = string.IsNullOrEmpty(spriteName) ? null : TextureManager.GetSprite(TextureDirectory.UI, spriteName)
            });
        }

        /// <summary>
        /// Add an option for a null value. Note: if one is already added, it will be overwriten.
        /// </summary>
        public void AddNoneOption(TMP_Dropdown.OptionData noneOption)
        {
            this.noneOption = noneOption;
            BaseDropdown.AddOption(noneOption);
            Items.Add(default(ObjectType));
            noneOptionIsVisible = true;
        }

        public void Clear()
        {
            BaseDropdown.Clear();
            Items.Clear();
            noneOption = null;
            noneOptionIsVisible = false;
        }

        public ObjectType Value
        {
            get {
                return Items.Count == 0 ? default(ObjectType) : Items[BaseDropdown.Value];
            }
            set {
                if (Items.Count != 0) {
                    if (value == null) {
                        if (!HasNoneOption) {
                            throw new ArgumentException("Dropdown has no value for null option");
                        }
                        BaseDropdown.Value = Items.IndexOf(default(ObjectType));
                    } else {
                        BaseDropdown.Value = Items.Any(item => item != null && item.InternalName == value.InternalName) ?
                            Items.IndexOf(Items.First(item => item != null && item.InternalName == value.InternalName)) : 0;
                    }
                }
            }
        }

        public int SelectedIndex
        {
            get {
                return BaseDropdown.Value;
            }
            set {
                if (HasNoneOption && !noneOptionIsVisible) {
                    int noneOptionIndex = Items.IndexOf(default(ObjectType));
                    if (value == noneOptionIndex) {
                        //Unhide none option
                        BaseDropdown.Clear();
                        foreach (ObjectType item in Items) {
                            if (item == null) {
                                BaseDropdown.AddOption(noneOption);
                                noneOptionIsVisible = true;
                            } else {
                                if (item.DropdownSprite == null) {
                                    BaseDropdown.AddOption(item.DropdownText);
                                } else {
                                    BaseDropdown.AddOption(item.DropdownText, item.DropdownSprite);
                                }
                            }
                        }
                        BaseDropdown.Value = value;
                    } else if (value > noneOptionIndex) {
                        //Shift index to match visible dropdown options
                        BaseDropdown.Value = value - 1;
                    }
                } else {
                    BaseDropdown.Value = value;
                }
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
            if (Items[index] != null && noneOptionIsVisible && HideNoneOption) {
                //Hide none option
                int noneOptionIndex = Items.IndexOf(default(ObjectType));
                BaseDropdown.Clear();
                foreach (ObjectType item in Items.Where(item => item != null)) {
                    AddBaseOption(item);
                }
                noneOptionIsVisible = false;
                BaseDropdown.Value = index > noneOptionIndex ? index - 1 : index;
            }
        }
    }

    public interface IDropdownOption
    {
        public string InternalName { get; }
        LString DropdownText { get; }
        /// <summary>
        /// Can be left as null, if no sprite is needed
        /// </summary>
        SpriteData DropdownSprite { get; }
    }
}
