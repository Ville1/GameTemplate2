using System;
using System.Collections.Generic;

namespace Game.Saving.Data
{
    [Serializable]
    public class Map : ISaveData
    {
        public int Width;
        public int Height;
        public List<Tile> Tiles;
    }
}
