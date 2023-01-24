using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Utils
{
    public class GameObjectHelper
    {

        public static GameObject Find(GameObject parent, string name)
        {
            return Find(parent.transform, name);
        }

        public static GameObject Find(Transform parent, string name)
        {
            for(int i = 0; i < parent.childCount; i++) {
                Transform transform = parent.GetChild(i);
                if(transform.gameObject.name == name) {
                    return transform.gameObject;
                }
            }
            return null;
        }

        public static GameObject Find(string parent, string name)
        {
            return GameObject.Find(string.Format("{0}/{1}", parent, name));
        }

        /// <summary>
        /// Find GameObject with using path of GameObject names
        /// </summary>
        /// <param name="parent">GameObject that is root for the path</param>
        /// <param name="gameObject">Found GameObject. If search fails, contains the last GameObject that could be found or null if none were found.</param>
        /// <param name="path">Path of GameObject names</param>
        /// <returns>True if last GameObject on the path was found, false otherwise</returns>
        public static bool FindWithPath(GameObject parent, out GameObject gameObject, params string[] path)
        {
            string placeholder1, placeholder2;
            return FindWithPath(parent, out gameObject, out placeholder1, out placeholder2, path.ToList());
        }

        /// <summary>
        /// Find GameObject with using path of GameObject names
        /// </summary>
        /// <param name="parent">GameObject that is root for the path</param>
        /// <param name="gameObject">Found GameObject. If search fails, contains the last GameObject that could be found or null if none were found.</param>
        /// <param name="lastParentName">Name of last parent GameObject that could be found</param>
        /// <param name="lastChildName">Name of last child GameObject that was search for (if result is false, this is the GameObject that could not be found)</param>
        /// <param name="path">Path of GameObject names</param>
        /// <returns>True if last GameObject on the path was found, false otherwise</returns>
        public static bool FindWithPath(GameObject parent, out GameObject gameObject, out string lastParentName, out string lastChildName, List<string> path)
        {
            //Check parameters
            if(parent == null) {
                throw new ArgumentException("No parent provided");
            }
            if(path == null || path.Count == 0) {
                throw new ArgumentException("No path provided");
            }

            bool success = true;
            gameObject = null;
            lastParentName = parent.name;
            lastChildName = path[0];

            //Loop through the path
            GameObject current = parent;
            foreach (string gameObjectName in path) {
                GameObject next = Find(current, gameObjectName);
                lastParentName = current.name;
                lastChildName = gameObjectName;
                if (next == null) {
                    success = false;
                    break;
                } else {
                    current = next;
                    gameObject = next;
                }
            }

            return success;
        }

        public static bool IsChild(GameObject parent, GameObject potentialChild)
        {
            int maxIterations = 100;
            GameObject currentParent = potentialChild;
            int iteration = 0;
            do {
                currentParent = currentParent.transform.parent != null ? currentParent.transform.parent.gameObject : null;
                if (currentParent == parent) {
                    return true;
                }
                iteration++;
            } while (currentParent != null && iteration < maxIterations);

            return false;
        }

        public static GameObject GetParent(GameObject gameObject)
        {
            return gameObject.transform.parent.gameObject;
        }

        public static void SetAnchorAndPivot(RectTransform rectTransform, Vector2 vector)
        {
            rectTransform.anchorMin = vector;
            rectTransform.anchorMax = vector;
            rectTransform.pivot = vector;
        }
    }
}
