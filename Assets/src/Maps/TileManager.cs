using Game.Objects;

namespace Game.Maps
{
    public class TileManager : PrototypeManager<Tile>
    {
        public TileManager() : base()
        {
            prototypes.Add(new Tile("Grass", "grass", 1.0f));
            prototypes.Add(new Tile("House", "house", 5.0f));
        }
    }
}
