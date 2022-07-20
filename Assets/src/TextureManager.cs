using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class TextureManager : MonoBehaviour
    {
        private static readonly string BASE_PATH = "textures";
        private static readonly bool PRELOAD_ALL = false;

        public static TextureManager Instance;

        private static TextureList<Texture2D> textures = new TextureList<Texture2D>();
        private static TextureList<Sprite> sprites = new TextureList<Sprite>();

        public static int LoadedCount { get { return textures.Count + sprites.Count; } }

        /// <summary>
        /// Initializiation
        /// </summary>
        private void Start()
        {
            if (Instance != null) {
                CustomLogger.Error("{AttemptingToCreateMultipleInstances}");
                return;
            }
            Instance = this;

            if (PRELOAD_ALL) {
                CustomLogger.Debug("{LoadingTextures}");
                textures.LoadAll();
                sprites.LoadAll();
                CustomLogger.Debug("{AllTexturesLoaded}");
            }
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        { }

        public static Sprite GetSprite(IHasSprite objectWithSprite)
        {
            return GetSprite(objectWithSprite.SpriteDirectory, objectWithSprite.Sprite);
        }

        public static Sprite GetSprite(TextureDirectory directory, string name)
        {
            return sprites.Get(directory, name);
        }

        public static Texture2D GetTexture2D(TextureDirectory directory, string name)
        {
            return textures.Get(directory, name);
        }

        private class TextureList<TTexture> where TTexture : UnityEngine.Object
        {
            public Dictionary<TextureDirectory, List<TextureListItem>> TexturesByDirectory { get; set; }
            public int Count { get { return TexturesByDirectory.Count == 0 ? 0 : TexturesByDirectory.Select(pair => pair.Value.Count).Sum(); } }

            public TextureList()
            {
                TexturesByDirectory = new Dictionary<TextureDirectory, List<TextureListItem>>();
            }

            public void LoadAll()
            {
                if(TexturesByDirectory.Count != 0) {
                    throw new Exception("Already loaded");
                }
                foreach (TextureDirectory directory in Enum.GetValues(typeof(TextureDirectory))) {
                    LoadTextures(directory);
                }
            }

            public bool Has(TextureDirectory directory, string textureName)
            {
                return TexturesByDirectory.ContainsKey(directory) && TexturesByDirectory[directory].Any(item => item.FullName == textureName);
            }

            public TTexture Get(TextureDirectory directory, string textureName)
            {
                if(PRELOAD_ALL && textureName.Contains('/')) {
                    //Remove subfolder paths if textures have been preloaded
                    textureName = textureName.Substring(textureName.LastIndexOf('/') + 1);
                }

                if(!Has(directory, textureName)) {
                    //Texture not found
                    if (PRELOAD_ALL) {
                        //Texture should already be loaded
                        CustomLogger.Error("{TextureNotFound}", typeof(TTexture).Name, directory.ToString().ToLower(), textureName);
                        return null;
                    } else {
                        //Load texture
                        TTexture texture = Resources.Load<TTexture>(string.Format("{0}/{1}/{2}", BASE_PATH, directory.ToString().ToLower(), textureName));
                        if(texture == null) {
                            //NOTE: Then using PRELOAD_ALL = false, you should include possible subdirectory path in textureName
                            //Example in TemplateProject: (Character.cs)
                            //AddAnimation(new SpriteAnimation("walk east", 10.0f, 0, "walk/stick figure walk {0}".Replicate(1, 4), TextureDirectory.Sprites));
                            CustomLogger.Error("{TextureNotFound}", typeof(TTexture).Name, directory.ToString().ToLower(), textureName);
                            return null;
                        }
                        if (!TexturesByDirectory.ContainsKey(directory)) {
                            TexturesByDirectory.Add(directory, new List<TextureListItem>());
                        }
                        TexturesByDirectory[directory].Add(new TextureListItem() {
                            Name = texture.name,
                            FullName = textureName,
                            Texture = texture
                        });
                        CustomLogger.Debug("{TextureLoaded}", typeof(TTexture).Name, texture.name);
                        return texture;
                    }
                }
                return TexturesByDirectory[directory].First(item => item.FullName == textureName).Texture as TTexture;
            }

            private void LoadTextures(TextureDirectory directory)
            {
                string path = string.Format("{0}/{1}", BASE_PATH, directory.ToString().ToLower());
                List<TextureListItem> list = new List<TextureListItem>();
                CustomLogger.Debug("{LoadingTexturesFromDirectory}", typeof(TTexture).Name, "/" + path);

                foreach (TTexture texture in Resources.LoadAll<TTexture>(path)) {
                    if (list.Any(item => item.FullName == texture.name)) {
                        CustomLogger.Warning("{DuplicatedTexture}", texture.name);
                    } else {
                        list.Add(new TextureListItem() {
                            Name = texture.name,
                            FullName = texture.name,
                            Texture = texture
                        });
                        CustomLogger.Debug("{TextureLoaded}", typeof(TTexture).Name, texture.name);
                    }
                }
                if (list.Count == 0) {
                    CustomLogger.Debug("{NoTextures}");
                }

                TexturesByDirectory.Add(directory, list);
            }
        }

        private class TextureListItem
        {
            public string FullName { get; set; }
            public string Name { get; set; }
            public UnityEngine.Object Texture { get; set; }
        }
    }

    public interface IHasSprite
    {
        string Sprite { get; }
        TextureDirectory SpriteDirectory { get; }
    }
}