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

        public enum MapState { Uninitialized, Generation, Ready }

        public MapState State { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<List<Tile>> Tiles { get; private set; }

        private int generationPositionX;
        private int generationPositionY;
        private EndGenerationCallback endGenerationCallback;
        private int generationTilesPerFrame;
        private WeightedRandomizer<Tile> tileRandomizer;
        private static List<Tile> tilePrototypes;
        private Tile draggedTile;
        private Tile dragOverTile;

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
            if(State == MapState.Ready) {
                //If map is already generated, clear old tiles
                foreach(Tile tile in this) {
                    tile.Destroy();
                }
                Tiles.Clear();
            }
            Active = true;
            State = MapState.Generation;
            generationPositionX = 0;
            generationPositionY = 0;
            Tiles = new List<List<Tile>>();
            Tiles.Add(new List<Tile>());
            this.endGenerationCallback = endGenerationCallback;
            generationTilesPerFrame = GENERATION_TILES_PER_FRAME_DEFAULT;

            tileRandomizer = new WeightedRandomizer<Tile>();
            tileRandomizer.Add(TilePrototypes[0], 100);
            tileRandomizer.Add(TilePrototypes[1], 10);
        }

        public void Save(ref ISaveData data)
        {
            Game.Saving.Data.Map saveData = (Game.Saving.Data.Map)data;
            saveData.Tiles.Add(new Saving.Data.Tile());
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
                generationPositionX,
                generationPositionY,
                tileRandomizer.Next()
            );
            Tiles[generationPositionY].Add(tile);

            //Add event listeners
            MouseManager.Instance.AddEventListerener(MouseButton.Left, MouseDragEventType.Start, new MouseDragEvent(tile, StartDragging));
            MouseManager.Instance.AddEventListerener(MouseButton.Left, MouseDragEventType.Move, new MouseDragEvent(tile, Drag));
            MouseManager.Instance.AddEventListerener(MouseButton.Left, MouseDragEventType.End, new MouseDragEvent(tile, EndDragging));

            //Move to next coordinates
            generationPositionX++;
            if (generationPositionX == Width) {
                //Move y-coordinate
                generationPositionY++;
                if (generationPositionY == Height) {
                    //End generation
                    return false;
                }
                //Add a new row
                generationPositionX = 0;
                Tiles.Add(new List<Tile>());
            }

            return true;
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