using Game.Input;
using Game.UI;
using UnityEngine;

namespace Game.Maps
{
    public class Tile : Object2D
    {
        public Map Map { get; private set; }
        public Coordinates Coordinates { get; private set; }
        public int X { get { return Coordinates == null ? 0 : Coordinates.X; } }
        public int Y { get { return Coordinates == null ? 0 : Coordinates.Y; } }
        public string Name { get; private set; }

        private GameObject rectangleGameObject = null;
        private bool rectangleNotFound = false;

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
            RectangleColor = null;
            MouseEventData.Tags.Add(MouseEventTag.IgnoreUI);
        }

        public Tile(Map map, int x, int y) : base(
            "Tile",
            string.Format("Tile_{0},{1}", x, y),
            true,
            new Vector3(map.gameObject.transform.position.x + (x * 1.0f), map.gameObject.transform.position.y + (y * 1.0f), map.gameObject.transform.position.z),
            map.transform,
            null,
            TextureDirectory.Terrain,
            MouseEventData.Default)
        {
            Map = map;
            Coordinates = new Coordinates(x, y);
            Name = "Unnamed";
            RectangleColor = null;
            MouseEventData.Tags.Add(MouseEventTag.IgnoreUI);
        }

        public Tile(string name, string spriteName) : base("Tile", string.Format("TilePrototype_{0}", name), spriteName, TextureDirectory.Terrain, MouseEventData.Default)
        {
            Name = name;
        }

        public void ChangeTo(Tile prototype)
        {
            Name = prototype.Name;
            Sprite = prototype.Sprite;
        }

        public override void OnClick(MouseButton button)
        {
            base.OnClick(button);
            UIManager.Instance.CloseAllWindows();
            //Utils.CustomLogger.DebugRaw(ToString() + " -> " + button.ToString());
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

        public void Load(Saving.Data.Tile saveData)
        {
            //TODO
        }

        private void FindRectangle()
        {
            rectangleGameObject = GameObject.Find(string.Format("{0}/Rectangle", GameObject.name));
            rectangleNotFound = rectangleGameObject == null;
        }
    }
}