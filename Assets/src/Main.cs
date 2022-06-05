using Game.Utils;
using UnityEngine;

namespace Game
{
    public class Main : MonoBehaviour
    {
        public static Main Instance;

        public State State { get; private set; }

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
            State = State.MainMenu;
            CustomLogger.Debug("GameStart");
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        { }
    }
}
