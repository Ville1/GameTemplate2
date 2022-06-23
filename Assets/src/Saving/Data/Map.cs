using System;
using System.Collections.Generic;

namespace Game.Saving.Data
{
    [Serializable]
    public class Map : ISaveData
    {
        public List<Tile> Tiles;
    }
}
