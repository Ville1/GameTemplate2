namespace Game.Maps
{
    public class Character : Object2D
    {
        public Tile Tile { get; private set; }

        public Character(Tile tile) : base("Character", true, tile.Position, tile.Map.transform, new SpriteData("stick figure", TextureDirectory.Sprites, 1), null, 10.0f)
        {
            Tile = tile;
            OnMovement.Add(() => { CameraManager.Instance.Center(this); });
        }

        public void Move(Direction direction)
        {
            if (IsMoving) {
                //Already moving
                return;
            }
            Tile newTile = Tile.Map.GetTileAt(Tile.Coordinates.Move(direction));
            if(newTile == null) {
                //Map edge
                return;
            }
            Tile = newTile;
            StartMoving(Tile.Position);
        }
    }
}
