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

        public SpriteData(string sprite, TextureDirectory spriteDirectory, int order = 0)
        {
            Sprite = sprite;
            SpriteDirectory = spriteDirectory;
            Order = order;
        }

        public SpriteData(SpriteData data)
        {
            Sprite = data.Sprite;
            SpriteDirectory = data.SpriteDirectory;
            Order = data.Order;
        }

        public void Set(SpriteData data)
        {
            Sprite = data.Sprite;
            SpriteDirectory = data.SpriteDirectory;
            Order = data.Order;
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
        /// <param name="framesPerSecond"></param>
        /// <param name="loopIndex">Index of sprite where animation restarts, if set to loop. This can be used to skip begining of the animation on successive loops.
        /// Set to 0 to play full animation or -1 to have animation not loop.</param>
        /// <param name="sprites"></param>
        /// <param name="spriteDirectory"></param>
        /// <exception cref="ArgumentException"></exception>
        public SpriteAnimation(float framesPerSecond, int loopIndex, List<string> sprites, TextureDirectory spriteDirectory)
        {
            if (framesPerSecond <= 0.0f) {
                throw new ArgumentException("framesPerSecond <= 0.0f");
            }
            if((loopIndex != -1 && loopIndex < 0) || loopIndex >= sprites.Count) {
                throw new ArgumentException("Invalid loopIndex, set to -1 for a non looping animation.");
            }
            FramesPerSecond = framesPerSecond;
            LoopIndex = loopIndex;
            Sprites = sprites;
            SpriteDirectory = spriteDirectory;
        }

        public SpriteAnimation(SpriteAnimation original)
        {
            FramesPerSecond = original.FramesPerSecond;
            LoopIndex = original.LoopIndex;
            Sprites = original.Sprites.Copy();
            SpriteDirectory = original.SpriteDirectory;
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
