using Game.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Maps
{
    public class Character : Object2D
    {
        public enum MovementType { Grid, Free, FreeNoRotation };

        private static bool HighlightTiles = true;

        public MovementType Movement { get; set; } = MovementType.Free;
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
            if (Movement != MovementType.Grid) {
                DebugWindowManager.Instance.SetValue("Player position", Position.ToString());
            }

            AddAnimation("wave", new SpriteAnimation(10.0f, 2, new List<string>() { "stick figure wave 1", "stick figure wave 2", "stick figure wave 3", "stick figure wave 4" }, TextureDirectory.Sprites));
        }

        public void Move(Direction direction)
        {
            if (Movement != MovementType.Grid) {
                if (movedThisFrame) {
                    return;
                }
                Vector3 lastPosition = new Vector3(Position.x, Position.y, Position.z);
                Move(direction.Quaternion, Movement == MovementType.Free);
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
                StopAnimation();
                DebugWindowManager.Instance.SetValue("Player tile", Tile.Coordinates.ToString());
                DebugWindowManager.Instance.SetValue("Player position", string.Format("{0} ({1})", Position, direction));
            } else {
                GridMove(direction);
            }
        }

        public void Wave()
        {
            if (IsPlayingAnimation) {
                StopAnimation();
            } else {
                //PlayAnimation("wave", () => { Utils.CustomLogger.Debug("Stopped waving :-("); });
                PlayAnimation("wave");
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
            StopAnimation();
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
