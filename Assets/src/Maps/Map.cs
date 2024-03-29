using Game.Input;
using Game.Objects;
using Game.Pathfinding;
using Game.Saving;
using Game.Saving.Data;
using Game.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Maps
{
    public class Map : MonoBehaviour, ISaveable, IEnumerable
    {
        private static readonly int GENERATION_TILES_PER_FRAME_DEFAULT = 10;
        private static readonly int GENERATION_TILES_PER_FRAME_DEFAULT_DELTA = 10;
        private static readonly float GENERATION_TARGET_FRAME_RATE = 60.0f;

        private static readonly bool DYNAMIC_PATHFINDING_NODES = true;
        private static readonly bool ENABLE_DIAGONAL_PATHFINDING = true;

        public delegate void EndGenerationCallback();

        public enum MapState { Uninitialized, Generation, Loading, Ready }

        public MapState State { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        /// <summary>
        /// Note: coordinates are flipped: Tiles[y][x]
        /// </summary>
        public List<List<Tile>> Tiles { get; private set; }
        public AStar<Tile> Pathfinding { get; private set; }

        private Coordinates generationPosition;
        private EndGenerationCallback endGenerationCallback;
        private int generationTilesPerFrame;
        private WeightedRandomizer<Tile> tileRandomizer;
        private static List<Tile> tilePrototypes;
        private Tile draggedTile;
        private Tile dragOverTile;
        private Coordinates savingPosition;
        private int loadingPosition;

        /// <summary>
        /// Initializiation
        /// </summary>
        private void Start()
        {

        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {
            if(State == MapState.Generation) {
                Generate();
            }
        }

        public bool Active
        {
            get {
                return gameObject.activeSelf;
            }
            set {
                gameObject.SetActive(value);
            }
        }

        public void StartGeneration(EndGenerationCallback endGenerationCallback)
        {
            Clear();
            Tiles.Add(new List<Tile>());

            State = MapState.Generation;
            generationPosition = new Coordinates(0, 0);
            this.endGenerationCallback = endGenerationCallback;
            generationTilesPerFrame = GENERATION_TILES_PER_FRAME_DEFAULT;

            tileRandomizer = new WeightedRandomizer<Tile>();
            tileRandomizer.Add(Prototypes.Tiles.Get("Grass"), 100);
            tileRandomizer.Add(Prototypes.Tiles.Get("House"), 10);
        }

        public void StartSaving(ref ISaveData data)
        {
            Game.Saving.Data.Map saveData = (Game.Saving.Data.Map)data;

            //Initialize save data
            saveData.Width = Width;
            saveData.Height = Height;
            saveData.Tiles = new List<Saving.Data.Tile>();
            savingPosition = new Coordinates(0, 0);
        }

        public float Save(ref ISaveData data)
        {
            Game.Saving.Data.Map saveData = (Game.Saving.Data.Map)data;

            //Save next tile
            saveData.Tiles.Add(Tiles[savingPosition.X][savingPosition.Y].GetSaveData());

            //Move to next tile
            if (savingPosition.MoveToNextInRectangle(0, Width, Height)) {
                float fullRows = savingPosition.Y * Width;
                float currentRow = savingPosition.X;
                return (fullRows + currentRow) / ((float)Width * Height);
            } else {
                return 1.0f;
            }
        }

        public void StartLoading(ISaveData data)
        {
            Game.Saving.Data.Map saveData = (Game.Saving.Data.Map)data;

            Clear();
            State = MapState.Loading;
            Width = saveData.Width;
            Height = saveData.Height;
            loadingPosition = 0;

            //Initialize tile lists with nulls, so it does not matter what order tiles are in save file
            //We can replace nulls in any order
            for(int y = 0; y < Height; y++) {
                Tiles.Add(new List<Tile>());
                for(int x = 0; x < Width; x++) {
                    Tiles[y].Add(null);
                }
            }
        }

        public float Load(ISaveData data)
        {
            Game.Saving.Data.Map saveData = (Game.Saving.Data.Map)data;
            Game.Saving.Data.Tile tileSaveData = saveData.Tiles[loadingPosition];
            Tile tile = Tile.Load(this, tileSaveData);
            Tiles[tileSaveData.Y][tileSaveData.X] = tile;
            SetEventListeners(tile);
            loadingPosition++;
            float progress = loadingPosition / (float)saveData.Tiles.Count;
            if(progress == 1.0f) {
                UpdatePathfindingNodes();//These should be included in progress bar updates
            }
            return progress;
        }

        public static Map Instantiate(string mapName, int width, int height)
        {
            if(string.IsNullOrEmpty(mapName) || width <= 0 || height <= 0) {
                throw new ArgumentException();
            }

            GameObject gameObject = new GameObject(mapName);
            Map map = gameObject.AddComponent<Map>();
            gameObject.transform.SetParent(GameObject.Find("/Maps").transform);
            map.State = MapState.Uninitialized;
            map.Width = width;
            map.Height = height;

            return map;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public MapEnumerator GetEnumerator()
        {
            return new MapEnumerator(Tiles, Width, Height);
        }

        public Tile GetTileAt(Coordinates coordinates)
        {
            return GetTileAt(coordinates.X, coordinates.Y);
        }

        public Tile GetTileAt(int x, int y)
        {
            if(x < 0 || y < 0 || x >= Width || y >= Height) {
                return null;
            }
            return Tiles[y][x];
        }

        public List<Tile> GetAdjacentTiles(Coordinates coordinates)
        {
            return GetTilesInRectangle(coordinates, 1).Where(tile => tile.Coordinates != coordinates).ToList();
        }

        public List<Tile> GetTilesInRectangle(Coordinates centerCoordinates, int size)
        {
            List<Tile> tiles = new List<Tile>();
            for (int x = centerCoordinates.X - size; x <= centerCoordinates.X + size; x++) {
                for (int y = centerCoordinates.Y - size; y <= centerCoordinates.Y + size; y++) {
                    Tile tile = GetTileAt(x, y);
                    if (tile != null) {
                        tiles.Add(tile);
                    }
                }
            }
            return tiles;
        }

        public List<Tile> GetTilesInLine(Coordinates coordinates1, Coordinates coordinates2)
        {
            List<Tile> tiles = new List<Tile>();

            //Add starting tile
            Tile startingTile = GetTileAt(coordinates1);
            if (startingTile == null) {
                //Starting tile is outside of the map
                return null;
            }

            //Calculate difference in coordinates
            int deltaX = coordinates2.X - coordinates1.X;
            int deltaY = coordinates2.Y - coordinates1.Y;

            if (deltaX == 0 || deltaY == 0 || Math.Abs(deltaX) == Math.Abs(deltaY)) {
                //Straight line
                Direction direction;
                if (deltaX == 0 || deltaY == 0) {
                    //Nondiagonal line
                    if (deltaX > 0) {
                        direction = Direction.East;
                    } else if (deltaX < 0) {
                        direction = Direction.West;
                    } else if (deltaY > 0) {
                        direction = Direction.North;
                    } else {
                        direction = Direction.South;
                    }
                } else {
                    //Diagonal line
                    if (deltaX > 0 && deltaY > 0) {
                        direction = Direction.NorthEast;
                    } else if (deltaX > 0 && deltaY < 0) {
                        direction = Direction.SouthEast;
                    } else if (deltaX < 0 && deltaY < 0) {
                        direction = Direction.SouthWest;
                    } else {
                        direction = Direction.NorthWest;
                    }
                }
                Coordinates nextCoordinates = new Coordinates(coordinates1).Move(direction);
                Tile newTile = GetTileAt(nextCoordinates);
                while (newTile != null && nextCoordinates != coordinates2) {
                    tiles.Add(newTile);
                    nextCoordinates = nextCoordinates.Move(direction);
                    newTile = GetTileAt(nextCoordinates);
                }
                return tiles;
            } else {
                //Select larger delta to be the "main" direction
                bool mainIsHorizontal;
                Direction mainDirection;
                Direction secondaryDirection;
                int mainDistance;
                int secondaryDistance;
                if (Math.Abs(deltaX) > Math.Abs(deltaY)) {
                    if (deltaX > 0) {
                        mainDirection = Direction.East;
                    } else {
                        mainDirection = Direction.West;
                    }
                    if (deltaY > 0) {
                        secondaryDirection = Direction.North;
                    } else {
                        secondaryDirection = Direction.South;
                    }
                    mainIsHorizontal = true;
                    mainDistance = Math.Abs(deltaX);
                    secondaryDistance = Math.Abs(deltaY);
                } else {
                    if (deltaY > 0) {
                        mainDirection = Direction.North;
                    } else {
                        mainDirection = Direction.South;
                    }
                    if (deltaX > 0) {
                        secondaryDirection = Direction.East;
                    } else {
                        secondaryDirection = Direction.West;
                    }
                    mainIsHorizontal = false;
                    mainDistance = Math.Abs(deltaY);
                    secondaryDistance = Math.Abs(deltaX);
                }
                Coordinates coordinates = new Coordinates(coordinates1);
                int currentStep = 0;
                int secondaryDistanceMoved = 0;
                do {
                    if (currentStep > (mainDistance + 1)) {
                        throw new Exception("Finding line failed: line has overshot the target");
                    }

                    if (coordinates.IsAdjacent(coordinates2)) {
                        //Next to the target, go to it next
                        coordinates = new Coordinates(coordinates2);
                    } else {
                        //Move towards the main direction
                        coordinates = coordinates.Move(mainDirection);

                        //Check if we still need to move in the secondary direction
                        if ((mainIsHorizontal && coordinates.Y != coordinates2.Y) || (!mainIsHorizontal && coordinates.X != coordinates2.X)) {
                            int targetDelta = Mathf.RoundToInt((currentStep / (float)(mainDistance + 1)) * secondaryDistance);
                            if (targetDelta > secondaryDistanceMoved) {
                                //Move in secondary direction
                                coordinates = coordinates.Move(secondaryDirection);
                                secondaryDistanceMoved++;
                            }
                        }
                    }

                    Tile tile = GetTileAt(coordinates);
                    if (tile == null) {
                        //Out of map
                        break;
                    }
                    tiles.Add(tile);
                    currentStep++;
                } while (coordinates != coordinates2);
            }

            return tiles;
        }

        public void Clear()
        {
            if (State == MapState.Ready) {
                //If map already has tiles, clear old tiles
                foreach (Tile tile in this) {
                    tile.DestroyGameObject();
                }
                Tiles.Clear();
            }
            Active = true;
            Tiles = new List<List<Tile>>();
            State = MapState.Uninitialized;
        }

        private void Generate()
        {
            //Adjust generation speed
            if(Main.Instance.CurrentFrameRate > GENERATION_TARGET_FRAME_RATE) {
                generationTilesPerFrame += Math.Min(1, Mathf.RoundToInt(((Main.Instance.CurrentFrameRate - GENERATION_TARGET_FRAME_RATE) / GENERATION_TARGET_FRAME_RATE) * GENERATION_TILES_PER_FRAME_DEFAULT_DELTA));
            }

            //Generate tiles
            for (int i = 0; i < generationTilesPerFrame; i++) {
                if (!GenerateNextTile()) {
                    //All tiles generated
                    EndGeneration();
                    return;
                }
            }

            //Update progressbar
            ProgressBar.Instance.Progress = Tiles.Select(row => row.Count).Sum() / ((float)Width * Height);
        }

        private bool GenerateNextTile()
        {
            //Instantiate a new tile
            Tile tile = new Tile(
                this,
                generationPosition.X,
                generationPosition.Y,
                tileRandomizer.Next()
            );
            Tiles[generationPosition.Y].Add(tile);

            //Add event listeners
            SetEventListeners(tile);

            //Move to next coordinates
            generationPosition.X++;
            if (generationPosition.X == Width) {
                //Move y-coordinate
                generationPosition.Y++;
                if (generationPosition.Y == Height) {
                    //End generation
                    return false;
                }
                //Add a new row
                generationPosition.X = 0;
                Tiles.Add(new List<Tile>());
            }

            return true;
        }

        private void SetEventListeners(Tile tile)
        {
            tile.RegisterDragEventListener(MouseDragEventType.Start, StartDragging);
            tile.RegisterDragEventListener(MouseDragEventType.Move, Drag);
            tile.RegisterDragEventListener(MouseDragEventType.End, EndDragging);
            tile.RegisterMouseOverEventListener(MouseOverEventType.Enter, (GameObject t) => {
                //Note: Event listener is registered with this tile as target, so parameter t is always tile's GameObject
                Utils.CustomLogger.Debug("Mouse enter: " + tile.ToString());
            });
            tile.RegisterMouseOverEventListener(MouseOverEventType.Over, (GameObject t) => {
                //Utils.CustomLogger.Debug("Mouse over: " + tile.ToString());
            });
            tile.RegisterMouseOverEventListener(MouseOverEventType.Exit, (GameObject t) => {
                Utils.CustomLogger.Debug("Mouse exit: " + tile.ToString());
            });
        }

        private void EndGeneration()
        {
            foreach(Tile tile in this) {
                tile.Active = true;
            }
            State = MapState.Ready;
            if(endGenerationCallback != null) {
                endGenerationCallback();
            }
            UpdatePathfindingNodes();//These should be included in progress bar updates
            //Input.MouseManager.Instance.AddEventListerener(MouseButton.Middle, new Input.MouseEvent(Tiles[0][0], (GameObject target) => { Utils.CustomLogger.DebugRaw("First tile middle click callback"); }, 1));
        }

        private void StartDragging(Vector3 vector, IClickListener draggedObject, IClickListener targetObject)
        {
            if(draggedObject is Tile) {
                draggedTile = draggedObject as Tile;
                draggedTile.RectangleColor = Color.gray;
            }
        }

        private void Drag(Vector3 vector, IClickListener draggedObject, IClickListener targetObject)
        {
            if(draggedTile == null) {
                return;
            }
            if(targetObject == null) {
                if(dragOverTile != null) {
                    dragOverTile.RectangleColor = null;
                    dragOverTile = null;
                }
                return;
            }
            Tile targetTile = targetObject as Tile;
            if(dragOverTile == targetTile || draggedTile == targetTile) {
                return;
            }
            targetTile.RectangleColor = draggedTile.Name == targetTile.Name ? Color.yellow : Color.blue;
            if (dragOverTile != null) {
                dragOverTile.RectangleColor = null;
            }
            dragOverTile = targetTile;
        }

        private void EndDragging(Vector3 vector, IClickListener draggedObject, IClickListener targetObject)
        {
            if (draggedTile == null) {
                return;
            }
            if (dragOverTile != null) {
                //Swap
                string draggedName = draggedTile.Name;
                string targetName = dragOverTile.Name;
                dragOverTile.ChangeTo(Prototypes.Tiles.Get(draggedName));
                draggedTile.ChangeTo(Prototypes.Tiles.Get(targetName));

                dragOverTile.RectangleColor = null;
                dragOverTile = null;
            }
            draggedTile.RectangleColor = null;
            draggedTile = null;
        }

        public void UpdatePathfindingNodes()
        {
            if(Pathfinding == null) {
                Pathfinding = new AStar<Tile>(
                    (PathfindingNode<Tile> node, PathfindingNode<Tile> end) => {
                        return node.Target.Coordinates.Distance(end.Target.Coordinates);
                    },
                    DYNAMIC_PATHFINDING_NODES ? (PathfindingNode<Tile> node) => {
                        return GetNeighbors(node.Target);
                    } : null
                );
            }

            if (!DYNAMIC_PATHFINDING_NODES) {
                foreach (List<Tile> list in Tiles) {
                    foreach (Tile tile in list) {
                        tile.PathfindingNode.Neighbors = GetNeighbors(tile);
                    }
                }
            }
        }

        private Dictionary<PathfindingNode<Tile>, double> GetNeighbors(Tile tile)
        {
            return Direction.Values.Where(direction => ENABLE_DIAGONAL_PATHFINDING || direction.IsDiagonal).Select(direction =>
                GetTileAt(tile.Coordinates.Move(direction))
            ).Where((t) => t != null).ToDictionary(
                t => t.PathfindingNode,
                t => t.MovementCost * (double)t.Coordinates.Distance(tile.Coordinates)
            );
        }

        /*public static List<Tile> TilePrototypes
        {
            get {
                if(tilePrototypes != null) {
                    return tilePrototypes;
                }
                tilePrototypes = new List<Tile>();

                tilePrototypes.Add(new Tile("Grass", "grass", 1.0f));
                tilePrototypes.Add(new Tile("House", "house", 5.0f));

                return tilePrototypes;
            }
        }*/
    }

    public class MapEnumerator : IEnumerator
    {
        private List<List<Tile>> tiles;
        private int width;
        private int height;
        private int x;
        private int y;

        public MapEnumerator(List<List<Tile>> tiles, int width, int height)
        {
            this.tiles = tiles;
            this.width = width;
            this.height = height;
            x = -1;
            y = 0;
        }

        public bool MoveNext()
        {
            x++;
            if(x == width) {
                y++;
                x = 0;
            }
            return y < height;
        }

        public void Reset()
        {
            x = -1;
            y = 0;
        }

        object IEnumerator.Current
        {
            get {
                return Current;
            }
        }

        public Tile Current
        {
            get {
                return tiles[x][y];
            }
        }
    }
}