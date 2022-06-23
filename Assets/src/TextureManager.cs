using Game.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class TextureManager : MonoBehaviour
    {
        private static readonly bool PRELOAD_ALL = true;

        public static TextureManager Instance;

        private static Dictionary<TextureDirectory, Dictionary<string, Texture2D>> textures = new Dictionary<TextureDirectory, Dictionary<string, Texture2D>>();
        private static Dictionary<TextureDirectory, Dictionary<string, Sprite>> sprites = new Dictionary<TextureDirectory, Dictionary<string, Sprite>>();

        /// <summary>
        /// Initializiation
        /// </summary>
        private void Start()
        {
            if (Instance != null) {
                CustomLogger.Error("AttemptingToCreateMultipleInstances");
                return;
            }
            Instance = this;

            if (PRELOAD_ALL) {
                CustomLogger.Debug("LoadingTextures");
                foreach (TextureDirectory folder in Enum.GetValues(typeof(TextureDirectory))) {
                    LoadTextures(folder);
                    LoadSprites(folder);
                }
                CustomLogger.Debug("AllTexturesLoaded");
            }
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        { }

        /// <summary>
        /// TODO: Runtime loading
        /// </summary>
        public static Texture2D GetTexture(TextureDirectory directory, string name)
        {
            if (!PRELOAD_ALL) {
                throw new NotImplementedException();
            }
            if (!textures[directory].ContainsKey(name)) {
                CustomLogger.Error("TextureNotFound", directory.ToString().ToLower(), name);
                return null;
            }
            return textures[directory][name];
        }

        public static Sprite GetSprite(IHasSprite objectWithSprite)
        {
            return GetSprite(objectWithSprite.SpriteDirectory, objectWithSprite.Sprite);
        }

        /// <summary>
        /// TODO: Runtime loading
        /// </summary>
        public static Sprite GetSprite(TextureDirectory directory, string name)
        {
            if (!PRELOAD_ALL) {
                throw new NotImplementedException();
            }
            if (!sprites[directory].ContainsKey(name)) {
                CustomLogger.Error("TextureNotFound", directory.ToString().ToLower(), name);
                return null;
            }
            return sprites[directory][name];
        }

        /// <summary>
        /// TODO: Allow these TextureDirectory - folders to have folder trees in them. Add paths to Dictionary keys?
        /// This might already be able to load files from subfolders, without any recursion shenanigans
        /// TODO: Capitalized directory names seem to cause weird issues, now they are all in lower case which does not look great
        /// </summary>
        private void LoadTextures(TextureDirectory directory)
        {
            string path = "textures/" + directory.ToString().ToLower();
            Dictionary<string, Texture2D> dictionary = new Dictionary<string, Texture2D>();
            textures.Add(directory, dictionary);

            CustomLogger.Debug("LoadingTexturesFromDirectory", typeof(Texture2D).Name, "/" + path);
            bool textureFound = false;
            foreach (Texture2D texture in Resources.LoadAll<Texture2D>(path)) {
                /*if (texture.name.Contains("/")) {
                    CustomLogger.Warning("UnsupportedTextureName");
                } else {*/
                if (dictionary.ContainsKey(texture.name)) {
                    CustomLogger.Warning("DuplicatedTexture", texture.name);
                } else {
                    dictionary.Add(texture.name, texture);
                    CustomLogger.Debug("TextureLoaded", typeof(Texture2D).Name, texture.name);
                }
                //}
                textureFound = true;
            }
            if (!textureFound) {
                CustomLogger.Debug("NoTextures");
            }
        }

        /// <summary>
        /// TODO: Duplicated code
        /// </summary>
        private void LoadSprites(TextureDirectory directory)
        {
            string path = "textures/" + directory.ToString().ToLower();
            Dictionary<string, Sprite> dictionary = new Dictionary<string, Sprite>();
            sprites.Add(directory, dictionary);

            CustomLogger.Debug("LoadingTexturesFromDirectory", typeof(Sprite).Name, "/" + path);
            bool textureFound = false;
            foreach (Sprite texture in Resources.LoadAll<Sprite>(path)) {
                /*if (texture.name.Contains("/")) {
                    CustomLogger.Warning("UnsupportedTextureName");
                } else {*/
                if (dictionary.ContainsKey(texture.name)) {
                    CustomLogger.Warning("DuplicatedTexture", texture.name);
                } else {
                    dictionary.Add(texture.name, texture);
                    CustomLogger.Debug("TextureLoaded", typeof(Sprite).Name, texture.name);
                }
                //}
                textureFound = true;
            }
            if (!textureFound) {
                CustomLogger.Debug("NoTextures");
            }
        }
    }

    public interface IHasSprite
    {
        string Sprite { get; }
        TextureDirectory SpriteDirectory { get; }
    }
}