using Game.Input;
using Game.Pathfinding;
using Game.UI;
using Game.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Maps
{
    public class Character : Object2D, IHasStats, IHasStatModifiers
    {
        public enum MovementType { Grid, Free, FreeNoRotation };

        private static bool HighlightTiles = true;

        public MovementType Movement { get; set; } = MovementType.Grid;
        public Tile Tile { get; private set; }
        public Stats Stats { get; private set; }
        public List<Equipment> Weapons { get; private set; }

        private Tile oldTile = null;
        private List<Tile> currentPath = null;

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

            AddAnimation(new SpriteAnimation("wave", 10.0f, 2, "wave/stick figure wave {0}".Replicate(1, 4), TextureDirectory.Sprites));
            AddAnimation(new SpriteAnimation("horn", 10.0f, null, "horn/stick figure horn {0}".Replicate(1, 5), TextureDirectory.Sprites));
            AddAnimation(new SpriteAnimation("walk east", 10.0f, 0, "walk/stick figure walk {0}".Replicate(1, 4), TextureDirectory.Sprites));
            AddAnimation(new SpriteAnimation("walk west", 10.0f, 0, "walk/stick figure walk {0}".Replicate(1, 4), TextureDirectory.Sprites, true));
            AddAnimation(new SpriteAnimation("stop", 2.0f, null, new List<string>() { "stick figure stop" }, TextureDirectory.Sprites));

            //MouseManager.Instance.AddEventListerener(MouseButton.Left, new MouseEvent((GameObject gameObject) => { Effect2DManager.Instance.Play("FlamePermanent", gameObject); }));

            Weapons = new List<Equipment>();
            /*Weapons.Add(new Equipment("Sword", new Stats(Stat.Strength, 1.0f)));
            Weapons.Add(new Equipment("Sword", new List<StatModifier>() { new StatModifier(Stat.TestStat, 10.0f) }));
            Weapons.Add(new Equipment("Sword2 electric boogaloo", new List<StatModifier>() { new StatModifier(Stat.Strength, 0.0f, 2.0f) }));
            Stats = new Stats(this, new Dictionary<Stat, float>() { { Stat.Strength, 5.0f }, { Stat.Dexterity, 10.0f }, { Stat.TestStat, 0.0f }, { Stat.Movement, 3.0f }, { Stat.HP, 0.0f } });
            Stats.Refill();

            Stats.HP -= 999.0f;
            Stats.HP.Amount = 80.0f;
            Stats.HP += 10.0f;
            Weapons.Add(new Equipment("Sword2 electric boogaloo", new List<StatModifier>() { new StatModifier(Stat.Strength, 0.0f, 2.0f) }));
            Stats.Update();

            Weapons.Add(new Equipment("Sword2 electric boogaloo", new List<StatModifier>() { new StatModifier(Stat.Movement, 1.0f, 2.0f) }));
            
            Stats.Strength.BaseValue += 1.0f;
            Stats.Strength += 1.0f;
            //Stats.Strength.Value += 1.0f;
            Stats.TestStat -= 100.0f;
            Stats.Update();
            float hp = Stats.HP;*/
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

        public List<StatModifier> GetStatModifiers()
        {
            List<StatModifier> list = new List<StatModifier>();
            foreach(Equipment weapon in Weapons) {
                list.AddRange(weapon.Stats);
            }
            return list;
        }

        public bool Path(Tile tile)
        {
            if(currentPath != null) {
                foreach(Tile t in currentPath) {
                    t.RectangleColor = null;
                }
                currentPath = null;
            }

            if(Tile == tile) {
                return false;
            }
            List<PathfindingNode<Tile>> path = Tile.Map.Pathfinding.Path(Tile.PathfindingNode, tile.PathfindingNode);
            if(path != null) {
                for(int i = 0; i < path.Count; i++) {
                    PathfindingNode<Tile> node = path[i];
                    node.Target.RectangleColor = Color.black;
                    if(i != 0 && !node.Target.Coordinates.IsAdjacent(path[i - 1].Target.Coordinates)) {
                        throw new System.Exception("Something went wrong with pathfinding!");
                    }
                    if (i != path.Count - 1 && !node.Target.Coordinates.IsAdjacent(path[i + 1].Target.Coordinates)) {
                        throw new System.Exception("Something went wrong with pathfinding!");
                    }
                }
                currentPath = path.Select(n => n.Target).ToList();
                return true;
            }
            return false;
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

        public class Equipment
        {
            public string Name { get; private set; }
            public List<StatModifier> Stats { get;private set; }

            public Equipment(string name, List<StatModifier> stats)
            {
                Name = name;
                Stats = stats.Select(modifier => { modifier.Name = name; return modifier; }).ToList();
            }
        }
    }
}
