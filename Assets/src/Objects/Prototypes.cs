using Game.Maps;

namespace Game.Objects
{
    public class Prototypes
    {
        public static TileManager tileManager;
        public static TileManager Tiles { get { if (tileManager == null) { tileManager = new TileManager(); } return tileManager; } }
    }
}
