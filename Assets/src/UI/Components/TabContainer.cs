using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Components
{
    public class TabContainer
    {
        protected readonly float DEFAULT_MENU_BUTTON_RIGHT_MARGIN = 1.0f;
        protected readonly float MENU_BUTTON_INNER_MARGIN = 5.0f;
        protected readonly float MENU_BUTTON_MIN_WIDTH = 100.0f;

        protected GameObject menuContainer;
        protected GameObject tabContainer;
        protected Button buttonPrototype;
        protected GameObject tabPrototype;
        protected float menuButtonMargin;

        protected int currentTab = -1;
        protected List<TabData> tabs = new List<TabData>();

        public TabContainer(GameObject menuContainer, GameObject tabContainer, Button buttonPrototype, GameObject tabPrototype, float? menuButtonMargin = null)
        {
            this.menuContainer = menuContainer;
            this.tabContainer = tabContainer;
            this.buttonPrototype = buttonPrototype;
            this.tabPrototype = tabPrototype;
            this.menuButtonMargin = menuButtonMargin.HasValue ? menuButtonMargin.Value : DEFAULT_MENU_BUTTON_RIGHT_MARGIN;

            this.buttonPrototype.gameObject.SetActive(false);
            this.tabPrototype.SetActive(false);
        }

        public void DeleteTabs()
        {
            foreach(TabData tab in tabs) {
                GameObject.Destroy(tab.GameObject);
                GameObject.Destroy(tab.Button.ButtonBase);
            }
            tabs.Clear();
            currentTab = -1;
        }

        public GameObject AddTab(LString title)
        {
            GameObject newTab = GameObject.Instantiate(
                tabPrototype,
                tabPrototype.transform.position.Clone(),
                Quaternion.identity,
                tabContainer.transform
            );
            int currentIndex = tabs.Count;
            newTab.name = "Tab " + currentIndex;

            CustomButton button = new CustomButton(buttonPrototype, menuContainer, new Vector2(tabs.Select(tab => tab.Button.Width + menuButtonMargin).Sum(), 0.0f), title, () => {
                CurrentTabIndex = currentIndex;
            });
            button.Active = true;
            button.Width = Mathf.Max(button.TmpText.preferredWidth + (2.0f * MENU_BUTTON_INNER_MARGIN), MENU_BUTTON_MIN_WIDTH);
            button.ButtonBase.name = "Tab Menu Button " + currentIndex;

            tabs.Add(new TabData() {
                Title = title,
                GameObject = newTab,
                Button = button
            });

            CurrentTabIndex = currentTab == -1 ? 0 : currentTab;
            return newTab;
        }

        public GameObject GetTab(int index)
        {
            return index < 0 || index >= tabs.Count ? null : tabs[index].GameObject;
        }

        public int CurrentTabIndex
        {
            get {
                return currentTab;
            }
            set {
                currentTab = Mathf.Clamp(value, -1, tabs.Count - 1);
                for (int i = 0; i < tabs.Count; i++) {
                    tabs[i].GameObject.SetActive(i == currentTab);
                    tabs[i].Button.Interactable = i != currentTab;
                }
            }
        }

        protected class TabData
        {
            public LString Title { get; set; }
            public CustomButton Button { get; set; }
            public GameObject GameObject { get; set; }
        }
    }

    public class EnumTabContainer<EnumType> : TabContainer where EnumType : Enum
    {
        public EnumTabContainer(GameObject menuContainer, GameObject tabContainer, Button buttonPrototype, GameObject tabPrototype, float? menuButtonMargin = null) :
            base(menuContainer, tabContainer, buttonPrototype, tabPrototype, menuButtonMargin)
        {}

        public EnumType CurrentTab
        {
            get {
                return (EnumType)Enum.Parse(typeof(EnumType), CurrentTabIndex.ToString());
            }
            set {
                CurrentTabIndex = (int)Convert.ChangeType(value, typeof(int));
            }
        }
    }
}
