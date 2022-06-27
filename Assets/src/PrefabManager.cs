using Game.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Game {
    public class PrefabManager : MonoBehaviour
    {
        private static readonly bool PRELOAD_ALL = true;

        public static PrefabManager Instance;

        private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

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
                CustomLogger.Debug("{LoadingPrefabs}");
                foreach(GameObject prefab in Resources.LoadAll<GameObject>("Prefabs")) {
                    if (prefabs.ContainsKey(prefab.name)) {
                        CustomLogger.Warning("{DuplicatedPrefab}", prefab.name);
                    } else {
                        prefabs.Add(prefab.name, prefab);
                        CustomLogger.Debug("{PrefabLoaded}", prefab.name);
                    }
                }
                CustomLogger.Debug("{AllPrefabsLoaded}");
            }
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        { }

        public GameObject Get(string name)
        {
            if (PRELOAD_ALL) {
                //All prefabs are loaded on game start, fetch prefab from dictionary
                if (!prefabs.ContainsKey(name)) {
                    CustomLogger.Error("{PrefabNotFound}", name);
                    return null;
                }
                return prefabs[name];
            }

            if (prefabs.ContainsKey(name)) {
                //Prefab was already loaded once, use reference in dictionary
                return prefabs[name];
            }
            //Load prefab
            GameObject prefab = Resources.Load<GameObject>(string.Format("Prefabs/{0}", name));
            if(prefab == null) {
                CustomLogger.Error("{PrefabNotFound}", name);
                return null;
            }
            CustomLogger.Debug("{PrefabLoaded}", name);
            prefabs.Add(name, prefab);
            return prefab;
        }
    }
}
