using Game.Input;
using UnityEngine;

namespace Game
{
    public class Object2DListener : MonoBehaviour, IClickListenerComponent
    {
        public Object2D Object2D { get; set; }
        public IClickListener Listener { get { return Object2D; } }

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