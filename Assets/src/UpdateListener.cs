using UnityEngine;

namespace Game
{
    public class UpdateListener : MonoBehaviour
    {
        public Object2D Object2D { get; set; }

        private void Start()
        { }


        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {
            if(Object2D != null) {
                Object2D.Update();
            }
        }
    }

}