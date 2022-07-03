using Game.Input;
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
        private int GENERATION_TILES_PER_FRAME_DEFAULT = 10;
        private int GENERATION_TILES_PER_FRAME_DEFAULT_DELTA = 10;
        private float GENERATION_TARGET_FRAME_RATE = 60.0f;

        public delegate void EndGenerationCallback();

        public enum MapState { Uninitialized, Generation, Loading, Ready }

        public MapState State { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        /// <summary>
        /// Note: coordinates are flipped: Tiles[y][x]
        /// </summary>
        public List<List<Tile>> Tiles { get; private set; }

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
            tileRandomizer.Add(TilePrototypes[0], 100);
            tileRandomizer.Add(TilePrototypes[1], 10);
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
            Tiles[tileSaveData.X][tileSaveData.Y] = tile;
            SetEventListeners(tile);
            loadingPosition++;
            return loadingPosition / (float)saveData.Tiles.Count;
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

        private void Clear()
        {
            if (State == MapState.Ready) {
                //If map already has tiles, clear old tiles
                foreach (Tile tile in this) {
                    tile.Destroy();
                    MouseManager.Instance.RemoveEventListerener(MouseButton.Left, MouseDragEventType.Start, new MouseDragEvent(tile));
                    MouseManager.Instance.RemoveEventListerener(MouseButton.Left, MouseDragEventType.Move, new MouseDragEvent(tile));
                    MouseManager.Instance.RemoveEventListerener(MouseButton.Left, MouseDragEventType.End, new MouseDragEvent(tile));
                }
                Tiles.Clear();
            }
            Active = true;
            Tiles = new List<List<Tile>>();
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
            MouseManager.Instance.AddEventListerener(MouseButton.Left, MouseDragEventType.Start, new MouseDragEvent(tile, StartDragging));
            MouseManager.Instance.AddEventListerener(MouseButton.Left, MouseDragEventType.Move, new MouseDragEvent(tile, Drag));
            MouseManager.Instance.AddEventListerener(MouseButton.Left, MouseDragEventType.End, new MouseDragEvent(tile, EndDragging));
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
            //Input.MouseManager.Instance.AddEventListerener(MouseButton.Middle, new Input.MouseEvent(Tiles[0][0], (GameObject target) => { Utils.CustomLogger.DebugRaw("First tile middle click callback"); }, 1));
        }

        private void StartDragging(Vector3 vector, IClickListener draggedObject, IClickListener targetObject)
        {
            draggedTile = draggedObject as Tile;
            draggedTile.RectangleColor = Color.gray;
        }

        private void Drag(Vector3 vector, IClickListener draggedObject, IClickListener targetObject)
        {
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
            if(dragOverTile != null) {
                dragOverTile.RectangleColor = null;
            }
            dragOverTile = targetTile;
        }

        private void EndDragging(Vector3 vector, IClickListener draggedObject, IClickListener targetObject)
        {
            if(dragOverTile != null) {
                //Swap
                string draggedName = draggedTile.Name;
                string targetName = dragOverTile.Name;
                dragOverTile.ChangeTo(TilePrototypes.First(tile => tile.Name == draggedName));
                draggedTile.ChangeTo(TilePrototypes.First(tile => tile.Name == targetName));

                dragOverTile.RectangleColor = null;
                dragOverTile = null;
            }
            draggedTile.RectangleColor = null;
            draggedTile = null;
        }

        public static List<Tile> TilePrototypes
        {
            get {
                if(tilePrototypes != null) {
                    return tilePrototypes;
                }
                tilePrototypes = new List<Tile>();

                tilePrototypes.Add(new Tile("Grass", "grass"));
                tilePrototypes.Add(new Tile("House", "house"));

                return tilePrototypes;
            }
        }
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