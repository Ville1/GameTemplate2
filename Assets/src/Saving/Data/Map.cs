using System.Collections.Generic;

namespace Game.Saving.Data
{
    public class Map : ISaveData
    {
        public List<Tile> Tiles;

        public Map()
        {
            Tiles = new List<Tile>();
        }
    }
}
