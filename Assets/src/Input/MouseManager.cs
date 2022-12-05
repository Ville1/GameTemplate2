using Game.UI;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Input
{

    public class MouseManager : MonoBehaviour
    {
        public delegate void OnClickNothingDelegate();

        public static MouseManager Instance;

        private Dictionary<MouseButton, MouseClickEventListener> mouseClickEvents = DictionaryHelper.CreateNewFromEnum((MouseButton button) => { return new MouseClickEventListener(button); });
        private Dictionary<MouseButton, MouseNothingClickEventListener> mouseNothingClickEvents = DictionaryHelper.CreateNewFromEnum((MouseButton button) => { return new MouseNothingClickEventListener(button); });
        private Dictionary<MouseButton, Dictionary<MouseDragEventType, MouseDragEventListener>> mouseDragEvents = new Dictionary<MouseButton, Dictionary<MouseDragEventType, MouseDragEventListener>>();
        private Dictionary<MouseButton, bool> buttonsDownLastFrame = DictionaryHelper.CreateNewFromEnum<MouseButton, bool>(false);
        private Dictionary<MouseButton, bool> buttonsHeldLastFrame = DictionaryHelper.CreateNewFromEnum<MouseButton, bool>(false);
        private Dictionary<MouseButton, GameObject> draggedObjects = DictionaryHelper.CreateNewFromEnum<MouseButton, GameObject>((MouseButton b) => { return null; });
        private Vector2 mouseScreenPositionLastFrame = new Vector2(0.0f, 0.0f);
        private Vector3 mouseWorldPositionLastFrame = new Vector3(0.0f, 0.0f, 0.0f);

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

            foreach(MouseButton button in Enum.GetValues(typeof(MouseButton))) {
                mouseDragEvents.Add(button, DictionaryHelper.CreateNewFromEnum((MouseDragEventType drag) => { return new MouseDragEventListener(button, drag); }));
            }
        }

        /// <summary>
        /// Per frame update
        /// TODO: Drag events don't get blocked by UI event as IsBlockedByUI is by default true
        /// </summary>
        private void Update()
        {
            //Mouse clicks can be listened in two ways:
            //1. Create an object which implements IClickListener (such as Object2D) and give it `public override void OnClick` - method
            //2. Use AddEventListener to add a listener to any GameObject (it needs to have BoxCollider tho, so it can register clicks)

            Dictionary<MouseButton, bool> buttonsDownThisFrame = DictionaryHelper.CreateNewFromEnum<MouseButton, bool>(false);
            Dictionary<MouseButton, bool> buttonsHeldThisFrame = DictionaryHelper.CreateNewFromEnum<MouseButton, bool>(false);
            Dictionary<MouseButton, bool> buttonsUpThisFrame = DictionaryHelper.CreateNewFromEnum<MouseButton, bool>(false);
            Dictionary<MouseButton, GameObject> rayCastHits = DictionaryHelper.CreateNewFromEnum<MouseButton, GameObject>((MouseButton b) => { return null; });
            Vector2 mouseScreenPosition = new Vector2(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y);

            //Button status
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton))) {
                buttonsDownThisFrame[button] = UnityEngine.Input.GetMouseButtonDown((int)button);
                buttonsHeldThisFrame[button] = UnityEngine.Input.GetMouseButton((int)button);
                buttonsUpThisFrame[button] = UnityEngine.Input.GetMouseButtonUp((int)button);
            }

            //Ray cast
            RaycastHit hit;
            if (Physics.Raycast(CameraManager.Instance.Camera.ScreenPointToRay(UnityEngine.Input.mousePosition), out hit)) {
                foreach (MouseButton button in Enum.GetValues(typeof(MouseButton))) {
                    if (buttonsDownThisFrame[button]) {
                        //Click events
                        mouseClickEvents[button].Activate(hit.transform.gameObject);
                    }
                    rayCastHits[button] = hit.transform.gameObject;
                }
            } else {
                //Clicked nothing
                foreach (MouseButton button in Enum.GetValues(typeof(MouseButton))) {
                    if (buttonsDownThisFrame[button]) {
                        mouseNothingClickEvents[button].Activate();
                    }
                }
            }

            //Drag events
            foreach(MouseButton button in Enum.GetValues(typeof(MouseButton))) {
                //Start events
                if (buttonsDownLastFrame[button] && buttonsHeldThisFrame[button]) {
                    //Untargeted events
                    mouseDragEvents[button][MouseDragEventType.Start].Activate(mouseWorldPositionLastFrame);
                    if (rayCastHits[button] != null) {
                        //Targeted events
                        mouseDragEvents[button][MouseDragEventType.Start].Activate(mouseWorldPositionLastFrame, rayCastHits[button], null);
                        draggedObjects[button] = rayCastHits[button];
                    }
                }
                //End events
                if (buttonsUpThisFrame[button] && buttonsHeldLastFrame[button]) {
                    //Untargeted events
                    mouseDragEvents[button][MouseDragEventType.End].Activate(MouseWorldPosition);
                    if (draggedObjects[button] != null) {
                        //Targeted events
                        mouseDragEvents[button][MouseDragEventType.End].Activate(MouseWorldPosition, draggedObjects[button], rayCastHits[button]);
                        draggedObjects[button] = null;
                    }
                }
                //Move events
                if (buttonsHeldLastFrame[button] && buttonsHeldThisFrame[button] && (MouseWorldPosition.x != mouseWorldPositionLastFrame.x || MouseWorldPosition.y != mouseWorldPositionLastFrame.y)) {
                    //Untargeted events
                    Vector3 difference = mouseWorldPositionLastFrame - MouseWorldPosition;
                    mouseDragEvents[button][MouseDragEventType.Move].Activate(difference);
                    if (draggedObjects[button] != null) {
                        //Targeted events
                        mouseDragEvents[button][MouseDragEventType.Move].Activate(difference, draggedObjects[button], rayCastHits[button]);
                    }
                }
            }

            //Update last frame data
            mouseScreenPositionLastFrame = mouseScreenPosition;
            mouseWorldPositionLastFrame = MouseWorldPosition;
            buttonsDownLastFrame = buttonsDownThisFrame;
            buttonsHeldLastFrame = buttonsHeldThisFrame;
        }

        public void AddEventListener(MouseButton button, MouseEvent mouseEvent)
        {
            mouseClickEvents[button].Add(mouseEvent);
        }

        public bool RemoveEventListener(MouseButton button, MouseEvent mouseEvent)
        {
            return mouseClickEvents[button].Remove(mouseEvent.Id);
        }

        public bool RemoveEventListener(MouseButton button, Guid eventId)
        {
            return mouseClickEvents[button].Remove(eventId);
        }

        public void AddEventListener(MouseButton button, MouseNothingClickEvent mouseEvent)
        {
            mouseNothingClickEvents[button].Add(mouseEvent);
        }

        public bool RemoveEventListener(MouseButton button, MouseNothingClickEvent mouseEvent)
        {
            return mouseNothingClickEvents[button].Remove(mouseEvent.Id);
        }

        public bool RemoveNothingClickEventListener(MouseButton button, Guid eventId)
        {
            return mouseNothingClickEvents[button].Remove(eventId);
        }

        public void AddEventListener(MouseButton button, MouseDragEventType dragEventType, MouseDragEvent mouseDragEvent)
        {
            mouseDragEvents[button][dragEventType].Add(mouseDragEvent);
        }

        public bool RemoveEventListener(MouseButton button, MouseDragEventType dragEventType, MouseDragEvent mouseDragEvent)
        {
            return mouseDragEvents[button][dragEventType].Remove(mouseDragEvent.Id);
        }

        public bool RemoveEventListener(MouseButton button, MouseDragEventType dragEventType, Guid dragEventId)
        {
            return mouseDragEvents[button][dragEventType].Remove(dragEventId);
        }

        public Vector3 MouseWorldPosition
        {
            get {
                return CameraManager.Instance.CurrentCamera.ScreenToWorldPoint(new Vector3(
                    UnityEngine.Input.mousePosition.x,
                    UnityEngine.Input.mousePosition.y,
                    CameraManager.Instance.CurrentCamera.transform.position.z
                ));
            }
        }

        public int ClickListenerCount
        {
            get {
                int count = 0;
                foreach(KeyValuePair<MouseButton, MouseClickEventListener> pair in mouseClickEvents) {
                    count += pair.Value.Events.Count;
                }
                return count;
            }
        }

        public int NothingClickListenerCount
        {
            get {
                int count = 0;
                foreach (KeyValuePair<MouseButton, MouseNothingClickEventListener> pair in mouseNothingClickEvents) {
                    count += pair.Value.Events.Count;
                }
                return count;
            }
        }

        public int DragListenerCount
        {
            get {
                int count = 0;
                foreach (KeyValuePair<MouseButton, Dictionary<MouseDragEventType, MouseDragEventListener>> pair in mouseDragEvents) {
                    foreach(KeyValuePair<MouseDragEventType, MouseDragEventListener> pair2 in pair.Value) {
                        count += pair2.Value.Events.Count;
                    }
                }
                return count;
            }
        }

        public int TotalListenerCount
        {
            get {
                return ClickListenerCount + NothingClickListenerCount + DragListenerCount;
            }
        }

        private class MouseClickEventListener
        {
            public MouseButton Button { get; set; }
            public List<MouseEvent> Events { get; set; }

            public MouseClickEventListener(MouseButton button)
            {
                Button = button;
                Events = new List<MouseEvent>();
            }

            public void Activate(GameObject target)
            {
                IClickListenerComponent component = target.GetComponent<IClickListenerComponent>();
                IClickListener listener = component != null ? component.Listener : null;
                if (Events.Count == 0) {
                    //No event listeners
                    if (listener != null && UIManager.Instance.CanFire(listener.MouseEventData)) {
                        //Target is linked to IClickListener
                        listener.OnClick(Button);
                    }
                } else {
                    bool onClickProced = false;
                    foreach (MouseEvent mouseEvent in Events) {
                        //Loop through all event listeners
                        if (listener != null && listener.MouseEventData.Priority >= mouseEvent.Priority && !onClickProced) {
                            //Target has IClickListener and now is right time to call it, based on priorities
                            if (UIManager.Instance.CanFire(listener.MouseEventData)) {
                                listener.OnClick(Button);
                            }
                            onClickProced = true;
                        }
                        if ((target == mouseEvent.Target || mouseEvent.Target == null) && UIManager.Instance.CanFire(mouseEvent.EventData)) {
                            //Call event listener
                            mouseEvent.Listener(target);
                        }
                    }
                    //Target has IClickListener and it has lower priority than all the event listeners, call it last
                    if (listener != null && !onClickProced && UIManager.Instance.CanFire(listener.MouseEventData)) {
                        listener.OnClick(Button);
                    }
                }
            }

            public void Add(MouseEvent mouseEvent)
            {
                Events.Add(mouseEvent);
                Events = Events.OrderByDescending(mouseEvent => mouseEvent.Priority).ToList();
            }

            public bool Remove(Guid eventId)
            {
                if(Events.Any(mouseEvent => mouseEvent.Id == eventId)) {
                    Events = Events.Where(mouseEvent => mouseEvent.Id != eventId).OrderByDescending(mouseEvent => mouseEvent.Priority).ToList();
                    return true;
                }
                return false;
            }
        }

        private class MouseNothingClickEventListener
        {
            public MouseButton Button { get; set; }
            public List<MouseNothingClickEvent> Events { get; set; }

            public MouseNothingClickEventListener(MouseButton button)
            {
                Button = button;
                Events = new List<MouseNothingClickEvent>();
            }

            public void Activate()
            {
                foreach (MouseNothingClickEvent clickEvent in Events) {
                    if (UIManager.Instance.CanFire(clickEvent.EventData)) {
                        clickEvent.Listener();
                    }
                }
            }

            public void Add(MouseNothingClickEvent mouseEvent)
            {
                Events.Add(mouseEvent);
                Events = Events.OrderByDescending(mouseEvent => mouseEvent.Priority).ToList();
            }

            public bool Remove(Guid eventId)
            {
                if (Events.Any(mouseEvent => mouseEvent.Id == eventId)) {
                    Events = Events.Where(mouseEvent => mouseEvent.Id != eventId).OrderByDescending(mouseEvent => mouseEvent.Priority).ToList();
                    return true;
                }
                return false;
            }
        }

        private class MouseDragEventListener
        {
            public MouseButton ButtonType { get; set; }
            public MouseDragEventType DragType { get; set; }
            public List<MouseDragEvent> Events { get; set; }

            public MouseDragEventListener(MouseButton button, MouseDragEventType drag)
            {
                ButtonType = button;
                DragType = drag;
                Events = new List<MouseDragEvent>();
            }

            public void Activate(Vector3 vector, GameObject draggedObject, GameObject targetObject)
            {
                IClickListenerComponent draggedComponent = draggedObject.GetComponent<IClickListenerComponent>();
                IClickListener draggedListener = draggedComponent != null ? draggedComponent.Listener : null;
                IClickListenerComponent targetComponent = targetObject != null ? targetObject.GetComponent<IClickListenerComponent>() : null;
                IClickListener targetListener = targetComponent != null ? targetComponent.Listener : null;

                foreach (MouseDragEvent dragEvent in Events) {
                    if(UIManager.Instance.CanFire(dragEvent.EventData) && dragEvent.GameObjectTarget == draggedObject) {
                        if (dragEvent.Targeting == MouseDragEvent.TargetType.ClickListener && draggedListener != null) {
                            dragEvent.ClickableListener(vector, draggedListener, targetListener);
                        } else if (dragEvent.Targeting == MouseDragEvent.TargetType.GameObject) {
                            dragEvent.GameObjectListener(vector, draggedObject, targetObject);
                        }
                    }
                }
            }

            public void Activate(Vector3 vector)
            {
                foreach (MouseDragEvent dragEvent in Events) {
                    if (dragEvent.Targeting == MouseDragEvent.TargetType.NoTarget) {
                        dragEvent.TargetlessListener(vector);
                    }
                }
            }

            public void Add(MouseDragEvent mouseEvent)
            {
                Events.Add(mouseEvent);
                Events = Events.OrderByDescending(mouseEvent => mouseEvent.Priority).ToList();
            }

            public bool Remove(Guid eventId)
            {
                if(Events.Any(mouseEvent => mouseEvent.Id == eventId)) {
                    Events = Events.Where(mouseEvent => mouseEvent.Id != eventId).OrderByDescending(mouseEvent => mouseEvent.Priority).ToList();
                    return true;
                }
                return false;
            }
        }
    }
}
