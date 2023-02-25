using Game.UI;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Input
{

    public class MouseManager : MonoBehaviour
    {
        public enum LogLevel { None, Basic, Verbose }

        public delegate void OnClickNothingDelegate();

        public static MouseManager Instance;

        public GameObject Canvas;

        public LogLevel DebugLogLevel = LogLevel.None;

        private Dictionary<MouseButton, MouseClickEventListener> mouseClickEvents = DictionaryHelper.CreateNewFromEnum((MouseButton button) => { return new MouseClickEventListener(button); });
        private Dictionary<MouseButton, MouseNothingClickEventListener> mouseNothingClickEvents = DictionaryHelper.CreateNewFromEnum((MouseButton button) => { return new MouseNothingClickEventListener(button); });
        private Dictionary<MouseButton, Dictionary<MouseDragEventType, MouseDragEventListener>> mouseDragEvents = new Dictionary<MouseButton, Dictionary<MouseDragEventType, MouseDragEventListener>>();
        private Dictionary<MouseOverEventType, MouseOverEventListener> mouseOverEvents = DictionaryHelper.CreateNewFromEnum((MouseOverEventType type) => { return new MouseOverEventListener(type); });
        private Dictionary<MouseButton, bool> buttonsDownLastFrame = DictionaryHelper.CreateNewFromEnum<MouseButton, bool>(false);
        private Dictionary<MouseButton, bool> buttonsHeldLastFrame = DictionaryHelper.CreateNewFromEnum<MouseButton, bool>(false);
        private Dictionary<MouseButton, GameObject> draggedObjects = DictionaryHelper.CreateNewFromEnum<MouseButton, GameObject>((MouseButton b) => { return null; });
        private Vector2 mouseScreenPositionLastFrame = new Vector2(0.0f, 0.0f);
        private Vector3 mouseWorldPositionLastFrame = new Vector3(0.0f, 0.0f, 0.0f);
        private UnityEngine.UI.GraphicRaycaster uiRaycaster;
        private GameObject lastFrameMouseOverObject = null;

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
            uiRaycaster = Canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
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

            //Raycast
            RaycastHit hit;
            bool hasHit = false;
            if (Physics.Raycast(CameraManager.Instance.Camera.ScreenPointToRay(UnityEngine.Input.mousePosition), out hit)) {
                foreach (MouseButton button in Enum.GetValues(typeof(MouseButton))) {
                    if (buttonsDownThisFrame[button]) {
                        //Click events
                        mouseClickEvents[button].Activate(hit.transform.gameObject);
                        Log("{0} click -> {1}", button, hit.transform.gameObject.name);
                    }
                    rayCastHits[button] = hit.transform.gameObject;
                }
                hasHit = true;
            } else {
                //Clicked nothing
                foreach (MouseButton button in Enum.GetValues(typeof(MouseButton))) {
                    if (buttonsDownThisFrame[button]) {
                        mouseNothingClickEvents[button].Activate();
                        Log("{0} click -> NOTHING", button);
                    }
                }
            }

            //UI raycast
            PointerEventData pointerEventData = new PointerEventData(null);
            pointerEventData.position = UnityEngine.Input.mousePosition;
            List<RaycastResult> uiRaycastResults = new List<RaycastResult>();
            uiRaycaster.Raycast(pointerEventData, uiRaycastResults);
            List<GameObject> uiRaycastResultObjects = uiRaycastResults.Select(hit => hit.gameObject).ToList();
            foreach (GameObject uiRaycastResult in uiRaycastResultObjects) {
                foreach (MouseButton button in Enum.GetValues(typeof(MouseButton))) {
                    if (buttonsDownThisFrame[button]) {
                        //Click events
                        mouseClickEvents[button].Activate(uiRaycastResult, uiRaycastResultObjects);
                        Log("{0} ui click -> {1}", button, uiRaycastResult.name);
                    }
                }
            }
            //TODO: UI dragging

            //Drag events
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton))) {
                //Start events
                if (buttonsDownLastFrame[button] && buttonsHeldThisFrame[button]) {
                    //Untargeted events
                    mouseDragEvents[button][MouseDragEventType.Start].Activate(mouseWorldPositionLastFrame);
                    LogVerbose("{0} drag start", button);
                    if (rayCastHits[button] != null) {
                        //Targeted events
                        LogVerbose("{0} drag start -> {1}", button, rayCastHits[button].name);
                        mouseDragEvents[button][MouseDragEventType.Start].Activate(mouseWorldPositionLastFrame, rayCastHits[button], null);
                        draggedObjects[button] = rayCastHits[button];
                    }
                }
                //End events
                if (buttonsUpThisFrame[button] && buttonsHeldLastFrame[button]) {
                    //Untargeted events
                    mouseDragEvents[button][MouseDragEventType.End].Activate(MouseWorldPosition);
                    LogVerbose("{0} drag end", button);
                    if (draggedObjects[button] != null) {
                        //Targeted events
                        LogVerbose("{0} drag end -> \"{1}\" dropped on \"{2}\"", button, draggedObjects[button].name, rayCastHits[button].name);
                        mouseDragEvents[button][MouseDragEventType.End].Activate(MouseWorldPosition, draggedObjects[button], rayCastHits[button]);
                        draggedObjects[button] = null;
                    }
                }
                //Move events
                if (buttonsHeldLastFrame[button] && buttonsHeldThisFrame[button] && (MouseWorldPosition.x != mouseWorldPositionLastFrame.x || MouseWorldPosition.y != mouseWorldPositionLastFrame.y)) {
                    //Untargeted events
                    Vector3 difference = mouseWorldPositionLastFrame - MouseWorldPosition;
                    mouseDragEvents[button][MouseDragEventType.Move].Activate(difference);
                    LogVerbose("{0} drag move -> difference: ({1}, {2}, {3})", button, difference.x, difference.y, difference.z);
                    if (draggedObjects[button] != null) {
                        //Targeted events
                        mouseDragEvents[button][MouseDragEventType.Move].Activate(difference, draggedObjects[button], rayCastHits[button]);
                        LogVerbose("{0} drag move -> \"{1}\" hover on \"{2}\"", button, draggedObjects[button].name, rayCastHits[button].name);
                    }
                }
            }

            //Mouse over events
            if (hasHit) {
                if(lastFrameMouseOverObject == null) {
                    mouseOverEvents[MouseOverEventType.Enter].Activate(hit.transform.gameObject);
                } else {
                    if(lastFrameMouseOverObject == hit.transform.gameObject) {
                        Vector3 difference = mouseWorldPositionLastFrame - MouseWorldPosition;
                        if (difference.x != 0.0f || difference.y != 0.0f || difference.z != 0.0f) {
                            mouseOverEvents[MouseOverEventType.Over].Activate(hit.transform.gameObject);
                        }
                    } else {
                        mouseOverEvents[MouseOverEventType.Exit].Activate(lastFrameMouseOverObject);
                        mouseOverEvents[MouseOverEventType.Enter].Activate(hit.transform.gameObject);
                    }
                }
            } else if(!hasHit && lastFrameMouseOverObject != null) {
                mouseOverEvents[MouseOverEventType.Exit].Activate(lastFrameMouseOverObject);
            }
            lastFrameMouseOverObject = hasHit ? hit.transform.gameObject : null;

            //Update last frame data
            mouseScreenPositionLastFrame = mouseScreenPosition;
            mouseWorldPositionLastFrame = MouseWorldPosition;
            buttonsDownLastFrame = buttonsDownThisFrame;
            buttonsHeldLastFrame = buttonsHeldThisFrame;
        }

        public Guid AddEventListener(MouseEvent mouseEvent)
        {
            foreach(MouseButton button in Enum.GetValues(typeof(MouseButton))) {
                AddEventListener(button, mouseEvent);
            }
            return mouseEvent.Id;
        }

        public Guid AddEventListener(MouseButton button, MouseEvent mouseEvent)
        {
            mouseClickEvents[button].Add(mouseEvent);
            return mouseEvent.Id;
        }

        public bool RemoveEventListener(MouseButton button, MouseEvent mouseEvent)
        {
            return mouseClickEvents[button].Remove(mouseEvent.Id);
        }

        public bool RemoveEventListener(MouseButton button, Guid eventId)
        {
            return mouseClickEvents[button].Remove(eventId);
        }

        public Guid AddEventListener(MouseNothingClickEvent mouseEvent)
        {
            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton))) {
                AddEventListener(button, mouseEvent);
            }
            return mouseEvent.Id;
        }

        public Guid AddEventListener(MouseButton button, MouseNothingClickEvent mouseEvent)
        {
            mouseNothingClickEvents[button].Add(mouseEvent);
            return mouseEvent.Id;
        }

        public bool RemoveEventListener(MouseButton button, MouseNothingClickEvent mouseEvent)
        {
            return mouseNothingClickEvents[button].Remove(mouseEvent.Id);
        }

        public bool RemoveNothingClickEventListener(MouseButton button, Guid eventId)
        {
            return mouseNothingClickEvents[button].Remove(eventId);
        }

        public Guid AddEventListener(MouseButton button, MouseDragEventType dragEventType, MouseDragEvent mouseDragEvent)
        {
            mouseDragEvents[button][dragEventType].Add(mouseDragEvent);
            return mouseDragEvent.Id;
        }

        public bool RemoveEventListener(MouseButton button, MouseDragEventType dragEventType, MouseDragEvent mouseDragEvent)
        {
            return mouseDragEvents[button][dragEventType].Remove(mouseDragEvent.Id);
        }

        public bool RemoveEventListener(MouseButton button, MouseDragEventType dragEventType, Guid dragEventId)
        {
            return mouseDragEvents[button][dragEventType].Remove(dragEventId);
        }

        public Guid AddEventListener(MouseOverEventType mouseOverEventType, MouseOverEvent mouseEvent)
        {
            mouseOverEvents[mouseOverEventType].Add(mouseEvent);
            return mouseEvent.Id;
        }

        public bool RemoveEventListener(MouseOverEventType mouseOverEventType, MouseOverEvent mouseEvent)
        {
            return mouseOverEvents[mouseOverEventType].Remove(mouseEvent.Id);
        }

        public bool RemoveEventListener(MouseOverEventType mouseOverEventType, Guid eventId)
        {
            return mouseOverEvents[mouseOverEventType].Remove(eventId);
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

        private void Log(string message, params object[] arguments)
        {
            if(DebugLogLevel >= LogLevel.Basic) {
                CustomLogger.Debug(string.Format(message, arguments));
            }
        }

        private void LogVerbose(string message, params object[] arguments)
        {
            if (DebugLogLevel >= LogLevel.Verbose) {
                CustomLogger.Debug(string.Format(message, arguments));
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

            /// <summary>
            /// </summary>
            /// <param name="target"></param>
            /// <param name="otherUIEventHits">If checking for ui click events, this contains list of other ui elements under cursor, target being one of them.</param>
            public void Activate(GameObject target, List<GameObject> otherUIEventHits = null)
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
                        if ((target == mouseEvent.Target || mouseEvent.Target == null) && UIManager.Instance.CanFire(mouseEvent.EventData, target, otherUIEventHits)) {
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

        private class MouseOverEventListener
        {
            public MouseOverEventType Type { get; set; }
            public List<MouseOverEvent> Events { get; set; }

            public MouseOverEventListener(MouseOverEventType type)
            {
                Events = new List<MouseOverEvent>();
                Type = type;
            }

            public void Activate(GameObject target)
            {
                if (Events.Count != 0) {
                    foreach (MouseOverEvent mouseEvent in Events) {
                        //Loop through all event listeners
                        if ((target == mouseEvent.Target || mouseEvent.Target == null) && UIManager.Instance.CanFire(mouseEvent.EventData, target, null)) {
                            //Call event listener
                            mouseEvent.Listener(target);
                        }
                    }
                }
            }

            public void Add(MouseOverEvent mouseEvent)
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
    }
}
