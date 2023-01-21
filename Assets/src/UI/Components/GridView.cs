using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Components
{
    public class GridView
    {
        private static readonly string DEFAULT_VIEWPORT_NAME = "Viewport";
        private static readonly string DEFAULT_CONTENT_NAME = "Content";
        private static readonly string DEFAULT_CELL_PROTOTYPE_NAME = "Cell Prototype";
        private static float DEFAULT_SENSITIVITY = 10.0f;

        public enum GridFillOrder { Vertical, Horizontal };

        public GameObject ScrollView { get; private set; }
        public GameObject Viewport { get; private set; }
        public GameObject Content { get; private set; }
        public RectTransform ContentRectTransform { get; private set; }
        public Scrollbar HorizontalScrollbar { get; private set; }
        public Scrollbar VerticalScrollbar { get; private set; }
        public ScrollRect ScrollRect { get; private set; }
        public ScrollRect.ScrollbarVisibility VerticalScrollbarVisibility { get; private set; }
        public ScrollRect.ScrollbarVisibility HorizontalScrollbarVisibility { get; private set; }
        public bool InitializationFailed { get; private set; }
        /// <summary>
        /// Max width in cells
        /// </summary>
        public int MaxWidth { get; set; }
        /// <summary>
        /// Max height in cells
        /// </summary>
        public int MaxHeight { get; set; }
        public GridFillOrder FillOrder { get; set; }
        public float CellSpacingX { get; set; }
        public float CellSpacingY { get; set; }

        private Dictionary<string, GameObject> cellPrototypes;
        private List<CellData> cells;
        private Coordinates lastCoordinates;

        public GridView(GameObject scrollView, GridViewParameters parameters = null)
        {
            Initialize(scrollView, null, null, null, null, null, null, null, null, parameters);
        }

        public GridView(GameObject scrollView, GameObject viewport, GameObject content, GameObject cellPrototype, GridViewParameters parameters = null)
        {
            Initialize(scrollView, viewport, null, content, null, cellPrototype, null, null, null, parameters);
        }

        public GridView(GameObject scrollView, GameObject viewport, GameObject content, List<GameObject> cellPrototypes, GridViewParameters parameters = null)
        {
            Initialize(scrollView, viewport, null, content, null, null, null, cellPrototypes, null, parameters);
        }

        public GridView(GameObject scrollView, string viewportName, string contentName, string cellPrototypeName, GridViewParameters parameters = null)
        {
            Initialize(scrollView, null, viewportName, null, contentName, null, cellPrototypeName, null, null, parameters);
        }

        public GridView(GameObject scrollView, string viewportName, string contentName, List<string> cellPrototypeNames, GridViewParameters parameters = null)
        {
            Initialize(scrollView, null, viewportName, null, contentName, null, null, null, cellPrototypeNames, parameters);
        }

        private void Initialize(GameObject scrollView, GameObject viewport, string viewportName, GameObject content, string contentName, GameObject cellPrototype, string cellPrototypeName,
            List<GameObject> cellPrototypeList, List<string> cellPrototypeNames, GridViewParameters parameters)
        {
            ScrollView = scrollView;
            parameters = parameters ?? new GridViewParameters();
            cellPrototypeList = cellPrototypeList ?? new List<GameObject>();
            cellPrototypeNames = cellPrototypeNames ?? new List<string>();

            //Set viewport
            Viewport = SetGameObject(ScrollView, viewport, viewportName, DEFAULT_VIEWPORT_NAME);
            if(Viewport == null) {
                return;
            }

            //Set content
            Content = SetGameObject(Viewport, content, contentName, DEFAULT_CONTENT_NAME);
            if(Content == null) {
                return;
            }

            //Set content RectTransform
            ContentRectTransform = Content.GetComponent<RectTransform>();
            if(ContentRectTransform == null) {
                CustomLogger.Warning("{ComponentNotFound}", Content.name, "RectTransform");
                return;
            }
            ContentRectTransform.anchorMin = new Vector2(0.0f, 1.0f);
            ContentRectTransform.anchorMax = new Vector2(0.0f, 1.0f);

            //Add cell prototypes to dictionary
            cellPrototypes = new Dictionary<string, GameObject>();
            if(cellPrototype != null) {
                cellPrototypeList.Add(cellPrototype);
            }
            if (!string.IsNullOrEmpty(cellPrototypeName)) {
                cellPrototypeNames.Add(cellPrototypeName);
            } else {
                cellPrototypeNames.Add(DEFAULT_CELL_PROTOTYPE_NAME);
            }

            foreach(GameObject parameterCellPrototype in cellPrototypeList) {
                if (!cellPrototypes.ContainsKey(parameterCellPrototype.name)) {
                    cellPrototypes.Add(parameterCellPrototype.name, parameterCellPrototype);
                }
            }

            foreach(string parameterCellPrototypeName in cellPrototypeNames) {
                if (!cellPrototypes.ContainsKey(parameterCellPrototypeName)) {
                    GameObject prototype = GameObjectHelper.Find(Content.transform, parameterCellPrototypeName);
                    if(prototype != null) {
                        cellPrototypes.Add(prototype.name, prototype);
                    }
                }
            }

            if(cellPrototypes.Count == 0) {
                //No prototypes
                CustomLogger.Error("{UIElementError}", "GridView has no cell prototypes");
                return;
            }
            foreach(KeyValuePair<string, GameObject> pair in cellPrototypes) {
                pair.Value.SetActive(false);
            }


            //Find scroll bars
            Scrollbar[] scrollbars = scrollView.GetComponentsInChildren<Scrollbar>();
            HorizontalScrollbar = scrollbars.FirstOrDefault(bar => bar.direction == Scrollbar.Direction.LeftToRight || bar.direction == Scrollbar.Direction.RightToLeft);
            VerticalScrollbar = scrollbars.FirstOrDefault(bar => bar.direction == Scrollbar.Direction.TopToBottom || bar.direction == Scrollbar.Direction.BottomToTop);

            //Set sensitivity
            ScrollRect = scrollView.GetComponent<ScrollRect>();
            ScrollRect.scrollSensitivity = parameters.ScrollSensitivity.HasValue ? parameters.ScrollSensitivity.Value : DEFAULT_SENSITIVITY;

            //Visibility
            ScrollRect.verticalScrollbarVisibility = parameters.VerticalScrollbarVisibility.HasValue ? parameters.VerticalScrollbarVisibility.Value : ScrollRect.verticalScrollbarVisibility;
            ScrollRect.horizontalScrollbarVisibility = parameters.HorizontalScrollbarVisibility.HasValue ? parameters.HorizontalScrollbarVisibility.Value : ScrollRect.horizontalScrollbarVisibility;

            //Settings
            MaxWidth = parameters.MaxWidth.HasValue ? parameters.MaxWidth.Value : int.MaxValue;
            MaxHeight = parameters.MaxHeight.HasValue ? parameters.MaxHeight.Value : int.MaxValue;
            FillOrder = parameters.FillOrder.HasValue ? parameters.FillOrder.Value : GridFillOrder.Horizontal;
            RectTransform prototypeRectTransform = cellPrototypes.First().Value.GetComponent<RectTransform>();
            CellSpacingX = parameters.CellSpacingX.HasValue ? parameters.CellSpacingX.Value : prototypeRectTransform.rect.width;
            CellSpacingY = parameters.CellSpacingY.HasValue ? parameters.CellSpacingY.Value : prototypeRectTransform.rect.height;

            cells = new List<CellData>();
            lastCoordinates = null;
        }

        private GameObject SetGameObject(GameObject parent, GameObject gameObject, string gameObjectName, string defaultGameObjectName)
        {
            if(gameObject == null) {
                gameObjectName = string.IsNullOrEmpty(gameObjectName) ? defaultGameObjectName : gameObjectName;
                gameObject = GameObjectHelper.Find(parent.transform, gameObjectName);
                if (gameObject == null) {
                    InitializationFailed = true;
                    CustomLogger.Error("{GameObjectNotFound}", gameObjectName, parent.name);
                }
            }

            return gameObject;
        }

        public GameObject AddCell(List<UIElementData> elementData, string prototypeName)
        {
            return AddCell(elementData, null, prototypeName);
        }

        public GameObject AddCell(List<UIElementData> elementData, Coordinates coordinates = null, string prototypeName = null)
        {
            if (InitializationFailed) {
                return null;
            }

            //Get prototype
            GameObject prototype;
            if (!string.IsNullOrEmpty(prototypeName)) {
                if (!cellPrototypes.ContainsKey(prototypeName)) {
                    CustomLogger.Error("{UIElementError}", string.Format("GridView has no prototype with name \"{0}\"", prototypeName));
                    return null;
                } else {
                    prototype = cellPrototypes[prototypeName];
                }
            } else {
                prototype = cellPrototypes.First().Value;
            }

            Coordinates nextCoordinates;
            if (coordinates == null) {
                //Calculate next coordinates
                nextCoordinates = lastCoordinates == null ? new Coordinates(0, 0) :
                    (FillOrder == GridFillOrder.Horizontal ? lastCoordinates.Move(Direction.East) : lastCoordinates.Move(Direction.North));

                if (nextCoordinates.X >= MaxWidth && FillOrder == GridFillOrder.Horizontal) {
                    //New row
                    nextCoordinates.X = 0;
                    nextCoordinates.Y++;
                    if (nextCoordinates.Y >= MaxHeight) {
                        //Out of space
                        return null;
                    }
                }

                if (nextCoordinates.Y >= MaxHeight && FillOrder == GridFillOrder.Vertical) {
                    //New column
                    nextCoordinates.Y = 0;
                    nextCoordinates.X++;
                    if (nextCoordinates.X >= MaxWidth) {
                        //Out of space
                        return null;
                    }
                }
            } else {
                //Check parameter coordinates
                nextCoordinates = new Coordinates(coordinates);
                if(nextCoordinates.X < 0 || nextCoordinates.Y < 0 || nextCoordinates.X >= MaxWidth || nextCoordinates.Y >= MaxHeight) {
                    throw new ArgumentException(string.Format("Invalid coordinates: {0}, {1}", nextCoordinates.X, nextCoordinates.Y));
                }
            }

            CellData preExistingCell = cells.FirstOrDefault(data => data.Coordinates == nextCoordinates);
            if (preExistingCell != null) {
                //There is already a cell here, delete it first
                GameObject.Destroy(preExistingCell.GameObject);
                cells = cells.Where(data => data.Coordinates != nextCoordinates).ToList();
            }

            //Create a new cell
            CellData data = new CellData() {
                Id = Guid.NewGuid(),
                Coordinates = nextCoordinates
            };
            GameObject cell = GameObject.Instantiate(
                prototype,
                new Vector3(
                    prototype.transform.position.x + (nextCoordinates.X * CellSpacingX),
                    prototype.transform.position.y + (-1.0f * nextCoordinates.Y * CellSpacingY),
                    prototype.transform.position.z
                ),
                Quaternion.identity,
                Content.transform
            );
            data.GameObject = cell;
            cell.name = string.Format("Cell ({0}, {1}) {2}", nextCoordinates.X, nextCoordinates.Y, data.Id);
            cell.SetActive(true);

            //Set elements
            foreach (UIElementData uiElementData in elementData) {
                uiElementData.Set(cell);
            }

            cells.Add(data);

            //Resize Content
            ContentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (cells.OrderByDescending(data => data.Coordinates.X).First().Coordinates.X + 1.0f) * CellSpacingX);
            ContentRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (cells.OrderByDescending(data => data.Coordinates.Y).First().Coordinates.Y + 1.0f) * CellSpacingY);

            lastCoordinates = nextCoordinates;
            return cell;
        }

        public void Clear()
        {
            foreach(CellData cellData in cells) {
                GameObject.Destroy(cellData.GameObject);
            }
            cells.Clear();
            lastCoordinates = null;
        }

        private class CellData
        {
            public Guid Id { get; set; }
            public Coordinates Coordinates { get; set; }
            public GameObject GameObject { get; set; }
        }
    }

    public class GridViewParameters
    {
        public ScrollRect.ScrollbarVisibility? VerticalScrollbarVisibility { get; set; } = null;
        public ScrollRect.ScrollbarVisibility? HorizontalScrollbarVisibility { get; set; } = null;
        public float? ScrollSensitivity { get; set; } = null;
        /// <summary>
        /// Max width in cells
        /// </summary>
        public int? MaxWidth { get; set; } = null;
        /// <summary>
        /// Max height in cells
        /// </summary>
        public int? MaxHeight { get; set; } = null;
        public GridView.GridFillOrder? FillOrder { get; set; } = null;
        public float? CellSpacingX { get; set; } = null;
        public float? CellSpacingY { get; set; } = null;
    }
}
