using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI.Components
{
    public class ScrollableList
    {
        private static long currentId = 0;

        private GameObject rowPrototype;
        private GameObject scrollViewContent;
        private List<Row> rows;
        private float rowSpacing;

        public ScrollableList(GameObject rowPrototype, GameObject scrollViewContent, float? rowSpacing = null)
        {
            if(rowPrototype == null || scrollViewContent == null) {
                throw new ArgumentNullException();
            }
            this.rowPrototype = rowPrototype;
            this.scrollViewContent = scrollViewContent;
            this.rowSpacing = rowSpacing.HasValue ? rowSpacing.Value : rowPrototype.GetComponent<RectTransform>().rect.height;
            rows = new List<Row>();
            rowPrototype.SetActive(false);
        }

        public GameObject AddRow(List<UIElementData> elementData)
        {
            return AddRow(new Row() { KeyInt = rows.Count, ElementData = elementData });
        }

        public GameObject AddRow(int key, List<UIElementData> elementData)
        {
            return AddRow(new Row() { KeyInt = key, ElementData = elementData });
        }

        public GameObject AddRow(long key, List<UIElementData> elementData)
        {
            return AddRow(new Row() { KeyLong = key, ElementData = elementData });
        }

        public GameObject AddRow(string key, List<UIElementData> elementData)
        {
            return AddRow(new Row() { KeyString = key, ElementData = elementData });
        }

        private GameObject AddRow(Row row)
        {
            GameObject gameObject = GameObject.Instantiate(
                rowPrototype,
                new Vector3(
                    rowPrototype.transform.position.x,
                    rowPrototype.transform.position.y + (rows.Count * rowSpacing),
                    rowPrototype.transform.position.z
                ),
                Quaternion.identity,
                scrollViewContent.transform
            );
            gameObject.SetActive(true);
            string name = string.Format("Row {0} (#{1})", row.Key, currentId);
            if(UIHelper.Find(scrollViewContent, name) != null) {
                throw new Exception(string.Format("Scroll view content already contains a row with name: '{0}'", name));
            }
            gameObject.name = name;
            currentId = currentId == long.MaxValue ? 0 : currentId + 1;
            foreach(UIElementData uiElementData in row.ElementData) {
                uiElementData.Set(gameObject);
            }
            row.GameObject = gameObject;
            rows.Add(row);
            return gameObject;
        }

        public void Clear()
        {
            foreach(Row row in rows) {
                GameObject.Destroy(row.GameObject);
            }
            rows.Clear();
        }

        private class Row
        {
            public int? KeyInt { get; set; }
            public long? KeyLong { get; set; }
            public string KeyString { get; set; }
            public List<UIElementData> ElementData { get; set; }
            public GameObject GameObject { get; set; }

            public string Key
            {
                get {
                    if (KeyInt.HasValue) {
                        return KeyInt.Value.ToString();
                    }
                    if (KeyLong.HasValue) {
                        return KeyLong.Value.ToString();
                    }
                    return KeyString;
                }
            }
        }
    }
}
