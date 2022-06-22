using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI.Components
{
    public class ScrollableList
    {
        private GameObject rowPrototype;
        private GameObject scrollViewContent;
        private List<Row> rows;


        public ScrollableList(GameObject rowPrototype, GameObject scrollViewContent)
        {
            if(rowPrototype == null || scrollViewContent == null) {
                throw new ArgumentNullException();
            }
            this.rowPrototype = rowPrototype;
            this.scrollViewContent = scrollViewContent;
            rows = new List<Row>();
        }

        public GameObject AddRow(List<UIElementData> elementData)
        {
            GameObject a = null;


            return a;
        }

        private class Row
        {
            public int KeyInt { get; set; }
            public int KeyLong { get; set; }
            public string KeyString { get; set; }
            public UIElementData ElementData { get; set; }
        }
    }
}
