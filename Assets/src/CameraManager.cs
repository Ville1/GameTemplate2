using Game.Input;
using Game.Utils;
using Game.Objects;
using UnityEngine;

namespace Game
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance;

        public Camera Camera;

        public float MovementSpeed { get; set; } = 10.0f;
        /// <summary>
        /// Multiplier
        /// </summary>
        public float DragSpeed { get; set; } = 1.0f;
        public Camera CurrentCamera { get; private set; }

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
            CurrentCamera = Camera;

            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.W, MoveUp, KeyEventTag.Camera);
            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.A, MoveLeft, KeyEventTag.Camera);
            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.S, MoveDown, KeyEventTag.Camera);
            KeyboardManager.Instance.AddKeyHeldEventListener(KeyCode.D, MoveRight, KeyEventTag.Camera);

            MouseManager.Instance.AddEventListener(MouseButton.Middle, MouseDragEventType.Move, new MouseDragEvent(Move));

            //Create CameraBackground
            //TODO: Add support for multiple cameras?
            GameObject background = new GameObject("Background");
            background.AddComponent<SpriteRenderer>();
            background.AddComponent<CameraBackground>();
            background.AddComponent<RectTransform>();
            background.GetComponent<RectTransform>().position = new Vector3(CurrentCamera.transform.position.x, CurrentCamera.transform.position.y, CurrentCamera.transform.position.z + CameraBackground.DISTANCE);
            background.GetComponent<RectTransform>().SetParent(CurrentCamera.transform);
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
            CurrentCamera.transform.Translate(Time.deltaTime * MovementSpeed * direction.Vector3);
        }

        public void Move(Vector3 delta)
        {
            Vector2 vector2 = new Vector2(-1.0f * delta.x, -1.0f * delta.y);
            CurrentCamera.transform.Translate(vector2);
        }

        public void Center(Object2D object2D)
        {
            Center(object2D.Position);
        }

        public void Center(Vector2 position)
        {
            CurrentCamera.transform.position = new Vector3(
                position.x,
                position.y,
                CurrentCamera.transform.position.z
            );
        }
    }
}
