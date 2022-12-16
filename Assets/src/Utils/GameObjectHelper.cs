using UnityEngine;

namespace Game.Utils
{
    public class GameObjectHelper
    {

        public static GameObject Find(Transform parent, string name)
        {
            return Find(parent.gameObject.name, name);
        }

        public static GameObject Find(GameObject parent, string name)
        {
            return Find(parent.name, name);
        }

        public static GameObject Find(string parent, string name)
        {
            return GameObject.Find(string.Format("{0}/{1}", parent, name));
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
    }
}
