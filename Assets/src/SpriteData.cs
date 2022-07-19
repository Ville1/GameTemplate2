using Game.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class SpriteData : IHasSprite
    {
        public string Sprite { get; set; }
        public TextureDirectory SpriteDirectory { get; set; }
        public int Order { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }

        public SpriteData(string sprite, TextureDirectory spriteDirectory, int order = 0, bool flipX = false, bool flipY = false)
        {
            Sprite = sprite;
            SpriteDirectory = spriteDirectory;
            Order = order;
            FlipX = flipX;
            FlipY = flipY;
        }

        public SpriteData(SpriteData data)
        {
            Sprite = data.Sprite;
            SpriteDirectory = data.SpriteDirectory;
            Order = data.Order;
            FlipX = data.FlipX;
            FlipY = data.FlipY;
        }

        public void Set(SpriteData data)
        {
            Sprite = data.Sprite;
            SpriteDirectory = data.SpriteDirectory;
            Order = data.Order;
            FlipX = data.FlipX;
            FlipY = data.FlipY;
        }

        public SpriteData Copy
        {
            get{
                return new SpriteData(this);
            }
        }
    }

    public class SpriteAnimation
    {
        public delegate void AnimationDelegate();

        public string Name { get; set; }
        public float FramesPerSecond { get; private set; }
        /// <summary>
        /// Index of sprite where animation restarts, if set to loop. This can be used to skip begining of the animation on successive loops.
        /// Set to 0 to play full animation or -1 to have animation not loop.
        /// </summary>
        public int LoopIndex { get; private set; }
        public bool IsLooping { get { return LoopIndex >= 0; } }
        public List<string> Sprites { get; private set; }
        public List<string> CurrentSprites { get; private set; }
        public TextureDirectory SpriteDirectory { get; private set; }
        public bool FlipX { get; private set; }
        public bool FlipY { get; private set; }
        public int CurrentSpriteIndex { get; private set; } = -1;
        public bool IsPlaying { get { return CurrentSpriteIndex >= 0; } }
        public SpriteData CurrentTarget { get; private set; } = null;
        public AnimationDelegate UpdateCallback { get; private set; } = null;
        public AnimationDelegate EndCallback { get; private set; } = null;

        private SpriteData originalSprite;
        private float currentFrameTimeLeft;
        private bool startRemoved;

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="framesPerSecond"></param>
        /// <param name="loopIndex">Index of sprite where animation restarts, if set to loop. This can be used to skip begining of the animation on successive loops.
        /// Set to 0 to play full animation or -1 / null to have animation not loop.</param>
        /// <param name="sprites"></param>
        /// <param name="spriteDirectory"></param>
        /// <param name="flipX"></param>
        /// <param name="flipY"></param>
        /// <exception cref="ArgumentException"></exception>
        public SpriteAnimation(string name, float framesPerSecond, int? loopIndex, List<string> sprites, TextureDirectory spriteDirectory, bool flipX = false, bool flipY = false)
        {
            if (framesPerSecond <= 0.0f) {
                throw new ArgumentException("framesPerSecond <= 0.0f");
            }
            int loopInd = loopIndex.HasValue ? loopIndex.Value : -1;
            if((loopInd != -1 && loopInd < 0) || loopInd >= sprites.Count) {
                throw new ArgumentException("Invalid loopIndex, set to -1 or null for a non looping animation.");
            }
            Name = name;
            FramesPerSecond = framesPerSecond;
            LoopIndex = loopInd;
            Sprites = sprites;
            SpriteDirectory = spriteDirectory;
            FlipX = flipX;
            FlipY = flipY;
        }

        public SpriteAnimation(SpriteAnimation original)
        {
            Name = original.Name;
            FramesPerSecond = original.FramesPerSecond;
            LoopIndex = original.LoopIndex;
            Sprites = original.Sprites.Copy();
            SpriteDirectory = original.SpriteDirectory;
            FlipX = original.FlipX;
            FlipY = original.FlipY;
        }

        /// <summary>
        /// </summary>
        /// <param name="endCallback">If provided, this gets called then animation ends. (Note: If animation is set to loop, this only gets called on Stop())</param>
        public void Start(AnimationDelegate endCallback)
        {
            Start(null, null, endCallback);
        }

        /// <summary>
        /// </summary>
        /// <param name="target">If provided, this object gets updated with new data as animation progresses</param>
        /// <param name="updateCallback">If provided, this gets called then sprite changes. (You can use this to update SpriteRenderer, or you can check return value of Update())</param>
        /// <param name="endCallback">If provided, this gets called then animation ends. (Note: If animation is set to loop, this only gets called on Stop())</param>
        public void Start(SpriteData target = null, AnimationDelegate updateCallback = null, AnimationDelegate endCallback = null)
        {
            CurrentSpriteIndex = 0;
            currentFrameTimeLeft = 1.0f / FramesPerSecond;
            startRemoved = false;
            CurrentSprites = Sprites.Copy();
            UpdateCallback = updateCallback;
            EndCallback = endCallback;

            if (target != null) {
                originalSprite = target.Copy;
                CurrentTarget = target;
                CurrentTarget.SpriteDirectory = SpriteDirectory;
                CurrentTarget.FlipX = FlipX;
                CurrentTarget.FlipY = FlipY;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>True if sprite has changed</returns>
        public bool Update()
        {
            if (!IsPlaying) {
                return false;
            }
            currentFrameTimeLeft -= Time.deltaTime;

            int increment = 0;
            while(currentFrameTimeLeft <= 0.0f) {
                //Next frame
                //Ideally increment should end up as 1, but if frame rate is low (currentFrameTimeLeft goes deep in negatives due to large Time.deltaTime),
                //we might need to skip some frames
                currentFrameTimeLeft += 1.0f / FramesPerSecond;
                increment++;
            }

            if(increment > 0) {
                //Increase index
                int oldIndex = CurrentSpriteIndex;
                CurrentSpriteIndex += increment;

                if(LoopIndex != 0 && IsLooping && !startRemoved && CurrentSpriteIndex >= LoopIndex) {
                    //Looping animation with a non zero LoopIndex has passed part that gets skipped on successive loops for the first time
                    //Remove start of animation from CurrentSprites
                    startRemoved = true;
                    CurrentSpriteIndex -= LoopIndex;
                    CurrentSprites.RemoveRange(0, LoopIndex);
                }

                while (CurrentSpriteIndex >= CurrentSprites.Count) {
                    //End of animation
                    if (IsLooping) {
                        //Loop back to start
                        CurrentSpriteIndex -= CurrentSprites.Count;
                    } else {
                        Stop();
                    }
                }

                if (IsPlaying && CurrentSpriteIndex != oldIndex) {
                    //Sprite changed and animation is still playing
                    if (CurrentTarget != null) {
                        CurrentTarget.Sprite = CurrentSprite;
                    }
                    if (UpdateCallback != null) {
                        UpdateCallback();
                    }
                }
                return true;
            }

            return false;
        }

        public string CurrentSprite
        {
            get {
                return IsPlaying ? CurrentSprites[CurrentSpriteIndex] : Sprites[0];
            }
        }

        public void Stop()
        {
            if(CurrentTarget != null) {
                CurrentTarget.Set(originalSprite);
                CurrentTarget = null;
                originalSprite = null;
            }
            if(UpdateCallback != null) {
                UpdateCallback();
            }
            if(EndCallback != null) {
                EndCallback();
            }

            UpdateCallback = null;
            EndCallback = null;
            CurrentSpriteIndex = -1;
        }
    }
}
