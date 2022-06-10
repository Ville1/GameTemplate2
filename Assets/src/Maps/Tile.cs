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

        public Tile(Map map, int x, int y, Tile prototype) : base(
            prototype,
            string.Format("Tile_{0},{1}", x, y),
            true,
            new Vector3(map.gameObject.transform.position.x + (x * 1.0f), map.gameObject.transform.position.y + (y * 1.0f), map.gameObject.transform.position.z),
            map.transform)
        {
            Map = map;
            Coordinates = new Coordinates(x, y);
        }

        public Tile(Map map, int x, int y) : base(
            "Tile",
            string.Format("Tile_{0},{1}", x, y),
            true,
            new Vector3(map.gameObject.transform.position.x + (x * 1.0f), map.gameObject.transform.position.y + (y * 1.0f), map.gameObject.transform.position.z),
            map.transform,
            null,
            TextureDirectory.Terrain)
        {
            Map = map;
            Coordinates = new Coordinates(x, y);
        }

        public Tile(string name, string spriteName) : base("Tile", string.Format("TilePrototype_{0}", name), spriteName, TextureDirectory.Terrain)
        {
            Name = name;
        }

        public override string ToString()
        {
            return IsPrototype ? string.Format("{0} Tile", Name) : string.Format("{0} Tile ({1},{2})", Name, X, Y);
        }
    }
}