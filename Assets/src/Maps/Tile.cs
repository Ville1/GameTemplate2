using Game.Input;
using Game.Objects;
using Game.Pathfinding;
using Game.UI;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Maps
{
    public class Tile : Object2D, IPrototypeable
    {
        private static readonly MouseButton DRAG_MOUSE_BUTTON = MouseButton.Left;

        public Map Map { get; private set; }
        public Coordinates Coordinates { get; private set; }
        public int X { get { return Coordinates == null ? 0 : Coordinates.X; } }
        public int Y { get { return Coordinates == null ? 0 : Coordinates.Y; } }
        public string Name { get; private set; }
        public string InternalName { get { return Name; } }
        public float MovementCost { get; private set; }
        public PathfindingNode<Tile> PathfindingNode { get; set; }

        private GameObject rectangleGameObject = null;
        private bool rectangleNotFound = false;
        private Dictionary<MouseDragEventType, List<Guid>> dragEvents = DictionaryHelper.CreateNewFromEnum((MouseDragEventType type) => { return new List<Guid>(); });
        private Dictionary<MouseOverEventType, List<Guid>> mouseOverEvents = DictionaryHelper.CreateNewFromEnum((MouseOverEventType type) => { return new List<Guid>(); });

        public Tile(Map map, int x, int y, Tile prototype) : base(
            prototype,
            string.Format("Tile_{0},{1}", x, y),
            true,
            new Vector3(map.gameObject.transform.position.x + (x * 1.0f), map.gameObject.transform.position.y + (y * 1.0f), map.gameObject.transform.position.z),
            map.transform)
        {
            Map = map;
            Coordinates = new Coordinates(x, y);
            Name = prototype.Name;
            MovementCost = prototype.MovementCost;
            RectangleColor = null;
            MouseEventData.Tags.Add(MouseEventTag.IgnoreUI);
            PathfindingNode = new PathfindingNode<Tile>() { Target = this };
        }

        public Tile(Map map, int x, int y) : base(
            "Tile",
            string.Format("Tile_{0},{1}", x, y),
            true,
            new Vector3(map.gameObject.transform.position.x + (x * 1.0f), map.gameObject.transform.position.y + (y * 1.0f), map.gameObject.transform.position.z),
            map.transform,
            new SpriteData(null, TextureDirectory.Terrain),
            MouseEventData.Default)
        {
            Map = map;
            Coordinates = new Coordinates(x, y);
            Name = "Unnamed";
            MovementCost = 1.0f;
            RectangleColor = null;
            MouseEventData.Tags.Add(MouseEventTag.IgnoreUI);
            PathfindingNode = new PathfindingNode<Tile>() { Target = this };
        }

        public Tile(string name, string spriteName, float movementCost) : base("Tile", string.Format("TilePrototype_{0}", name), new SpriteData(spriteName, TextureDirectory.Terrain), MouseEventData.Default)
        {
            Name = name;
            MovementCost = movementCost;
        }

        public IPrototypeable Clone
        {
            get {
                return new Tile(Name, Sprite, MovementCost);
            }
        }

        public void ChangeTo(Tile prototype)
        {
            Name = prototype.Name;
            Sprite = prototype.Sprite;
            MovementCost = prototype.MovementCost;
            Map.UpdatePathfindingNodes();
        }

        public override void OnClick(MouseButton button)
        {
            base.OnClick(button);
            UIManager.Instance.CloseAllWindows();

            if(button == MouseButton.Right && Main.Instance.PlayerCharacter != null) {
                Main.Instance.PlayerCharacter.Path(this);
            }
        }

        public override void Update()
        {
            base.Update();
            //Utils.CustomLogger.DebugRaw(ToString() + ": \"Hello, im spamming the log! :)\"");
        }

        public override string ToString()
        {
            return IsPrototype ? string.Format("{0} Tile", Name) : string.Format("{0} Tile ({1},{2})", Name, X, Y);
        }

        public Color? RectangleColor
        {
            get {
                if (rectangleNotFound) {
                    return null;
                }
                if(rectangleGameObject == null) {
                    FindRectangle();
                }
                return rectangleGameObject.activeSelf ? rectangleGameObject.GetComponent<SpriteRenderer>().color : null;
            }
            set {
                if (rectangleGameObject == null) {
                    FindRectangle();
                }
                if (rectangleNotFound) {
                    return;
                }
                if (value.HasValue) {
                    rectangleGameObject.SetActive(true);
                    rectangleGameObject.GetComponent<SpriteRenderer>().color = value.Value;
                } else {
                    rectangleGameObject.SetActive(false);
                }
            }
        }

        public Saving.Data.Tile GetSaveData()
        {
            Saving.Data.Tile saveData = new Saving.Data.Tile();
            saveData.X = X;
            saveData.Y = Y;
            saveData.Name = Name;
            return saveData;
        }

        public void RegisterDragEventListener(MouseDragEventType dragEventType, MouseDragEvent.OnDragDelegateClickable listener)
        {
            MouseDragEvent dragEvent = new MouseDragEvent(this, listener);
            MouseManager.Instance.AddEventListener(DRAG_MOUSE_BUTTON, dragEventType, dragEvent);
            dragEvents[dragEventType].Add(dragEvent.Id);
        }

        public void UnregisterDragEventListeners()
        {
            foreach(KeyValuePair<MouseDragEventType, List<Guid>> pair in dragEvents) {
                foreach(Guid id in pair.Value) {
                    MouseManager.Instance.RemoveEventListener(DRAG_MOUSE_BUTTON, pair.Key, id);
                }
                pair.Value.Clear();
            }
        }

        public void RegisterMouseOverEventListener(MouseOverEventType mouseOverEventType, MouseOverEvent.OnMouseOverDelegate listener)
        {
            MouseOverEvent overEvent = new MouseOverEvent(this, listener);
            MouseManager.Instance.AddEventListener(mouseOverEventType, overEvent);
            mouseOverEvents[mouseOverEventType].Add(overEvent.Id);
        }

        public void UnregisterMouseOverEventListeners()
        {
            foreach (KeyValuePair<MouseOverEventType, List<Guid>> pair in mouseOverEvents) {
                foreach (Guid id in pair.Value) {
                    MouseManager.Instance.RemoveEventListener(pair.Key, id);
                }
                pair.Value.Clear();
            }
        }

        public override void DestroyGameObject()
        {
            UnregisterDragEventListeners();
            UnregisterMouseOverEventListeners();
            base.DestroyGameObject();
        }

        public static Tile Load(Map map, Saving.Data.Tile saveData)
        {
            //TODO: Error handling. Save file can have tile names that don't match any prototypes
            return new Tile(map, saveData.X, saveData.Y, Prototypes.Tiles.Get(saveData.Name));
        }

        private void FindRectangle()
        {
            rectangleGameObject = GameObject.Find(string.Format("{0}/Rectangle", GameObject.name));
            rectangleNotFound = rectangleGameObject == null;
        }
    }
}