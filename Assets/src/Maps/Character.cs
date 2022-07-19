using Game.UI;
using Game.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Maps
{
    public class Character : Object2D
    {
        public enum MovementType { Grid, Free, FreeNoRotation };

        private static bool HighlightTiles = true;

        public MovementType Movement { get; set; } = MovementType.Grid;
        public Tile Tile { get; private set; }

        private Tile oldTile = null;

        public Character(Tile tile) : base("Character", true, tile.Position, tile.Map.transform, new SpriteData("stick figure", TextureDirectory.Sprites, 1), null, 1.0f)
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

            AddAnimation(new SpriteAnimation("wave", 10.0f, 2, "stick figure wave {0}".Replicate(1, 4), TextureDirectory.Sprites));
            AddAnimation(new SpriteAnimation("horn", 10.0f, null, "stick figure horn {0}".Replicate(1, 5), TextureDirectory.Sprites));
            AddAnimation(new SpriteAnimation("walk east", 10.0f, 0, "stick figure walk {0}".Replicate(1, 4), TextureDirectory.Sprites));
            AddAnimation(new SpriteAnimation("walk west", 10.0f, 0, "stick figure walk {0}".Replicate(1, 4), TextureDirectory.Sprites, true));
            AddAnimation(new SpriteAnimation("stop", 2.0f, null, new List<string>() { "stick figure stop" }, TextureDirectory.Sprites));
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
            if(CurrentAnimation == "wave") {
                StopAnimation(false);
            } else {
                //PlayAnimation("wave", () => { Utils.CustomLogger.Debug("Stopped waving :-("); });
                PlayAnimation("wave");
            }
        }

        public void Horn()
        {
            PlayAnimation("horn", AnimationQueue.QueueUnlimited);
        }

        public void Stop()
        {
            if (IsMoving) {
                EndMovement(true);
                if (HighlightTiles) {
                    Tile.RectangleColor = null;
                    oldTile.RectangleColor = Color.black;
                }
                Tile = oldTile;
                PlayAnimation("stop", hasMovementAnimation ? AnimationQueue.QueueOne : AnimationQueue.StopCurrent);
            } else {
                PlayAnimation("stop");
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
            StartMoving(Tile.Position, direction.Vector2.x >= 0 ? "walk east" : "walk west");
            DebugWindowManager.Instance.SetValue("Player tile", string.Format("{0} -> {1} ({2})", oldTile.Coordinates, Tile.Coordinates, direction));
        }

        public override void Update()
        {
            if (!movedLastFrame) {
                DebugWindowManager.Instance.SetValue("Player position", Position.ToString());
            }
            DebugWindowManager.Instance.SetValue("Player animation", currentAnimation == null ? "none" : currentAnimation.CurrentSprite);
            DebugWindowManager.Instance.SetValue("Animation queue", "(" + animationQueue.Count + "): " + string.Join(", ", animationQueue.Select(x => x.Name).ToList()));
            base.Update();
        }
    }
}
