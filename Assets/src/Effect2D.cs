using Game.Utils;
using System;
using UnityEngine;

namespace Game
{
    public class Effect2D : Object2D
    {
        private static readonly int SORTING_ORDER = 999;

        public string Name { get; private set; }
        public Guid? Id { get; private set; }
        public SpriteAnimation Animation { get; private set; }
        public float? Duration { get; private set; }
        public float? DurationRemaining { get; private set; }

        public Effect2D(Effect2D prototype, Guid id, string objectName, Vector3 position, Transform parent) : base(prototype, objectName, true, position, parent)
        {
            Name = prototype.Name;
            Id = id;
            Animation = new SpriteAnimation(prototype.Animation);
            Duration = prototype.Duration;
            DurationRemaining = prototype.Duration;
            AddAnimation(Animation);
            PlayAnimation(animations[0].Name, AnimationQueue.StopCurrent, () => { Effect2DManager.Instance.Remove(Id.Value); });
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="animation"></param>
        /// <param name="duration">Seconds</param>
        public Effect2D(string name, SpriteAnimation animation, float? duration) : base(null, name, new SpriteData(animation.Sprites[0], animation.SpriteDirectory, SORTING_ORDER))
        {
            Name = name;
            Id = null;
            Animation = new SpriteAnimation(animation);
            Duration = duration;
            DurationRemaining = null;
            if(Duration.HasValue && !animation.IsLooping) {
                Duration = null;
                CustomLogger.Warning("EffectRequiresLoopingAnimation");
            }
        }

        public override void Update()
        {
            base.Update();
            if (DurationRemaining.HasValue) {
                DurationRemaining -= Time.deltaTime;
                if(DurationRemaining <= 0.0f) {
                    StopAnimation();
                }
            }
        }
    }
}
