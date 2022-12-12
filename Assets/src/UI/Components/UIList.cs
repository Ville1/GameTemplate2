using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.UI.Components
{
    public class UIList
    {
        private static readonly string DEFAULT_ROW_PROTOTYPE_NAME = "Row Prototype";
        private static readonly bool SET_ROW_THROW_EXCEPTIONS = false;//If true, exception is thrown when using SetRow on nonexistent row. If false, AddRow is used instead.
        private static readonly string SET_ROW_EXCEPTION_MESSAGE = "Row with key '{0}' does not exist";

        private static long currentId = 0;

        protected GameObject rowPrototype;
        protected GameObject container;
        protected List<Row> rows;
        protected float rowSpacing;

        public UIList(GameObject rowPrototype, GameObject container, float? rowSpacing = null)
        {
            Initialize(rowPrototype, container, rowSpacing);
        }

        public UIList(string rowPrototypeName, GameObject container, float? rowSpacing = null)
        {
            Initialize(FindRowPrototype(container, rowPrototypeName), container, rowSpacing);
        }

        public UIList(GameObject container, float? rowSpacing = null)
        {
            Initialize(FindRowPrototype(container, DEFAULT_ROW_PROTOTYPE_NAME), container, rowSpacing);
        }

        private void Initialize(GameObject rowPrototype, GameObject container, float? rowSpacing = null)
        {
            if (rowPrototype == null || container == null) {
                throw new ArgumentNullException();
            }
            this.rowPrototype = rowPrototype;
            this.container = container;
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

        public GameObject AddRow(Guid key, List<UIElementData> elementData)
        {
            return AddRow(key.ToString(), elementData);
        }

        private GameObject AddRow(Row row)
        {
            GameObject gameObject = GameObject.Instantiate(
                rowPrototype,
                new Vector3(
                    rowPrototype.transform.position.x,
                    rowPrototype.transform.position.y + (rows.Count * -rowSpacing),
                    rowPrototype.transform.position.z
                ),
                Quaternion.identity,
                container.transform
            );
            gameObject.SetActive(true);
            string name = string.Format("Row {0} (#{1})", row.Key, currentId);
            if (UIHelper.Find(container, name) != null) {
                throw new Exception(string.Format("List container already contains a row with name: '{0}'", name));
            }
            gameObject.name = name;
            currentId = currentId == long.MaxValue ? 0 : currentId + 1;
            foreach (UIElementData uiElementData in row.ElementData) {
                uiElementData.Set(gameObject);
            }
            row.GameObject = gameObject;
            rows.Add(row);
            container.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rows.Count * rowSpacing);
            return gameObject;
        }

        public bool HasRow(int key)
        {
            return rows.Any(row => row.KeyInt == key);
        }

        public bool HasRow(long key)
        {
            return rows.Any(row => row.KeyLong == key);
        }

        public bool HasRow(string key)
        {
            return rows.Any(row => row.KeyString == key);
        }

        public bool HasRow(Guid key)
        {
            return HasRow(key.ToString());
        }

        private bool HasRow(Row row)
        {
            if (row.KeyInt.HasValue) {
                return HasRow(row.KeyInt.Value);
            }
            if (row.KeyLong.HasValue) {
                return HasRow(row.KeyLong.Value);
            }
            return HasRow(row.KeyString);
        }

        public void SetRow(int key, List<UIElementData> elementData)
        {
            SetRow(new Row() { KeyInt = key, ElementData = elementData });
        }

        public void SetRow(long key, List<UIElementData> elementData)
        {
            SetRow(new Row() { KeyLong = key, ElementData = elementData });
        }

        public void SetRow(string key, List<UIElementData> elementData)
        {
            SetRow(new Row() { KeyString = key, ElementData = elementData });
        }

        public void SetRow(Guid key, List<UIElementData> elementData)
        {
            SetRow(key.ToString(), elementData);
        }

        private void SetRow(Row row)
        {
            if (!HasRow(row)) {
                if (SET_ROW_THROW_EXCEPTIONS) {
                    throw new ArgumentException(string.Format(SET_ROW_EXCEPTION_MESSAGE, row.Key));
                } else {
                    AddRow(row);
                    return;
                }
            }
            GameObject gameObject = null;
            if (row.KeyInt.HasValue) {
                gameObject = rows.First(r => r.KeyInt == row.KeyInt).GameObject;
            } else if (row.KeyLong.HasValue) {
                gameObject = rows.First(r => r.KeyLong == row.KeyLong).GameObject;
            } else {
                gameObject = rows.First(r => r.KeyString == row.KeyString).GameObject;
            }

            foreach (UIElementData uiElementData in row.ElementData) {
                uiElementData.Set(gameObject);
            }
        }

        public void Clear()
        {
            foreach (Row row in rows) {
                row.Destroy();
            }
            rows.Clear();
        }

        public float Height
        {
            get {
                return container.GetComponent<RectTransform>().rect.height;
            }
        }

        private static GameObject FindRowPrototype(GameObject container, string name)
        {
            RectTransform[] rectTransforms = container.GetComponentsInChildren<RectTransform>();
            foreach (RectTransform rectTransform in rectTransforms) {
                if (rectTransform.gameObject.name == name) {
                    return rectTransform.gameObject;
                }
            }
            return null;
        }

        protected class Row
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

            public void Destroy()
            {
                if(ElementData != null) {
                    foreach(UIElementData tooltipData in ElementData.Where(elementData => elementData.Type == UIElementData.ElementType.Tooltip)) {
                        TooltipManager.Instance.UnregisterTooltip(UIHelper.Find(GameObject, tooltipData.GameObjectName));
                    }
                }
                UnityEngine.GameObject.Destroy(GameObject);
            }
        }
    }
}
