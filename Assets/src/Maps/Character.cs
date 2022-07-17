using Game.UI;
using UnityEngine;

namespace Game.Maps
{
    public class Character : Object2D
    {
        private enum MovementType { Grid, Free, FreeNoRotation };

        private static bool HighlightTiles = true;
        private static MovementType movementType = MovementType.Free;

        public Tile Tile { get; private set; }

        private Tile oldTile = null;

        public Character(Tile tile) : base("Character", true, tile.Position, tile.Map.transform, new SpriteData("stick figure", TextureDirectory.Sprites, 1), null, 10.0f)
        {
            Tile = tile;
            Tile.RectangleColor = Color.black;
            OnMovement.Add(() => {
                CameraManager.Instance.Center(this);
            });
            OnMovementEnd.Add(() => {
                if (HighlightTiles) {
                    oldTile.RectangleColor = null;
                }
                DebugWindowManager.Instance.SetValue("Player tile", Tile.Coordinates.ToString());
            });
            DebugWindowManager.Instance.SetValue("Player tile", Tile.Coordinates.ToString());
            if (movementType != MovementType.Grid) {
                DebugWindowManager.Instance.SetValue("Player position", Position.ToString());
            }
        }

        public void Move(Direction direction)
        {
            if (movementType != MovementType.Grid) {
                if (movedThisFrame) {
                    return;
                }
                Vector3 lastPosition = new Vector3(Position.x, Position.y, Position.z);
                Move(direction.Quaternion, movementType == MovementType.Free);
                Tile newTile = Tile.Map.GetTileAt(new Coordinates(Position));
                if(newTile != null) {
                    if (HighlightTiles) {
                        Tile.RectangleColor = null;
                        newTile.RectangleColor = Color.black;
                    }
                    Tile = newTile;
                } else {
                    //Map edge
                    Position = lastPosition;
                }
                DebugWindowManager.Instance.SetValue("Player tile", Tile.Coordinates.ToString());
                DebugWindowManager.Instance.SetValue("Player position", string.Format("{0} ({1})", Position, direction));
            } else {
                GridMove(direction);
            }
        }

        private void GridMove(Direction direction)
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
            oldTile = Tile;
            Tile = newTile;
            if (HighlightTiles) {
                oldTile.RectangleColor = Color.white;
                Tile.RectangleColor = Color.black;
            }
            StartMoving(Tile.Position);
            DebugWindowManager.Instance.SetValue("Player tile", string.Format("{0} -> {1} ({2})", oldTile.Coordinates, Tile.Coordinates, direction));
        }

        public override void Update()
        {
            if (!movedLastFrame) {
                DebugWindowManager.Instance.SetValue("Player position", Position.ToString());
            }
            base.Update();
        }
    }
}
