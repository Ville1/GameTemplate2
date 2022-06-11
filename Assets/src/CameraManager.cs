using Game.Input;
using Game.Utils;
using UnityEngine;

namespace Game
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance;

        public Camera Camera;

        public float MovementSpeed { get; set; } = 10.0f;

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

            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.W, MoveUp, KeyEventTag.Camera);
            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.A, MoveLeft, KeyEventTag.Camera);
            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.S, MoveDown, KeyEventTag.Camera);
            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.D, MoveRight, KeyEventTag.Camera);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        { }

        public void MoveUp()
        {
            Move(Direction.North);
        }

        public void MoveDown()
        {
            Move(Direction.South);
        }

        public void MoveLeft()
        {
            Move(Direction.West);
        }

        public void MoveRight()
        {
            Move(Direction.East);
        }

        public void Move(Direction direction)
        {
            Camera.transform.Translate(Time.deltaTime * MovementSpeed * direction.Vector3);
        }
    }
}
