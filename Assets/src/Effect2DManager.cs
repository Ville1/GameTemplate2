using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class Effect2DManager : MonoBehaviour
    {
        private static readonly bool CASE_SENSITIVE_NAMES = false;

        public static Effect2DManager Instance;

        public GameObject Container;

        private List<Effect2D> prototypes;
        private Dictionary<Guid, Effect2D> activeEffects;

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

            prototypes = new List<Effect2D>();
            prototypes.Add(new Effect2D("Flame", new SpriteAnimation("flame", 10.0f, -1, "flame_{0}".Replicate(1, 3), TextureDirectory.Effects), null));
            prototypes.Add(new Effect2D("Flame10s", new SpriteAnimation("flame", 10.0f, 0, "flame_{0}".Replicate(1, 3), TextureDirectory.Effects), 10.0f));
            prototypes.Add(new Effect2D("FlamePermanent", new SpriteAnimation("flame", 10.0f, 0, "flame_{0}".Replicate(1, 3), TextureDirectory.Effects), null));

            activeEffects = new Dictionary<Guid, Effect2D>();
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {

        }

        public Guid Play(string effectName, GameObject gameObjectPosition)
        {
            return Play(effectName, gameObjectPosition.transform.position);
        }

        public Guid Play(string effectName, Vector3 position)
        {
            Effect2D prototype = prototypes.FirstOrDefault(effect => effect.Name == effectName || (!CASE_SENSITIVE_NAMES && effect.Name.ToLower() == effectName.ToLower()));
            if(prototype == null) {
                CustomLogger.Warning("{EffectNotFound}", effectName);
                return Guid.Empty;
            }
            Guid id = Guid.NewGuid();
            activeEffects.Add(id, new Effect2D(prototype, id, string.Format("{0}_{1}", effectName, id), position, Container.transform));

            return id;
        }

        public bool Remove(Guid effectId)
        {
            if (!activeEffects.ContainsKey(effectId)) {
                return false;
            }

            Effect2D effect = activeEffects[effectId];
            effect.DestroyGameObject();
            activeEffects.Remove(effectId);

            return true;
        }

        public void RemoveAll()
        {
            foreach(KeyValuePair<Guid, Effect2D> pair in activeEffects) {
                pair.Value.DestroyGameObject();
            }
            activeEffects.Clear();
        }

        public int ActiveEffectCount
        {
            get {
                return activeEffects.Count;
            }
        }
    }
}
