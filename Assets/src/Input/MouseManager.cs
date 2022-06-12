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

        private Dictionary<MouseButton, MouseEventListener> mouseEvents = new Dictionary<MouseButton, MouseEventListener>();

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

            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton))) {
                mouseEvents.Add(button, new MouseEventListener(button));
            }
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {
            //Mouse clicks can be listened in two ways:
            //1. Create an object which implements IClickListener (such as Object2D) and give it `public override void OnClick` - method
            //2. Use AddEventListerener to add a listener to any GameObject (it needs to have BoxCollider tho, so it can register clicks)

            RaycastHit hit;
            if (Physics.Raycast(CameraManager.Instance.Camera.ScreenPointToRay(UnityEngine.Input.mousePosition), out hit)) {
                foreach (MouseButton button in Enum.GetValues(typeof(MouseButton))) {
                    if (UnityEngine.Input.GetMouseButtonDown((int)button)) {
                        mouseEvents[button].Activate(hit.transform.gameObject, button);
                    }
                }
            } else {
                //Add OnNothingClick listeners
            }
        }

        public void AddEventListerener(MouseButton button, MouseEvent mouseEvent)
        {
            mouseEvents[button].Add(mouseEvent);
        }

        public void RemoveEventListerener(MouseButton button, MouseEvent mouseEvent)
        {
            mouseEvents[button].Remove(mouseEvent);
        }

        private class MouseEventListener
        {
            public MouseButton Button { get; set; }
            public List<MouseEvent> Events { get; set; }

            public MouseEventListener(MouseButton button)
            {
                Button = button;
                Events = new List<MouseEvent>();
            }

            public void Activate(GameObject target, MouseButton button)
            {
                IClickListenerComponent component = target.GetComponent<IClickListenerComponent>();
                IClickListener listener = component != null ? component.Listener : null;
                if(Events.Count == 0) {
                    //No event listeners
                    if(listener != null && UIManager.Instance.CanFire(listener.MouseEventData.Tags)) {
                        //Target is linked to IClickListener
                        listener.OnClick(Button);
                    }
                } else {
                    bool onClickProced = false;
                    foreach (MouseEvent keyEvent in Events) {
                        //Loop through all event listeners
                        if(listener != null && listener.MouseEventData.Priority >= keyEvent.Priority && !onClickProced) {
                            //Target has IClickListener and now is right time to call it, based on priorities
                            if (UIManager.Instance.CanFire(listener.MouseEventData.Tags)) {
                                listener.OnClick(Button);
                            }
                            onClickProced = true;
                        }
                        if (target == keyEvent.Target && UIManager.Instance.CanFire(keyEvent.Tags)) {
                            //Call event listener
                            keyEvent.Listener(target, button);
                        }
                    }
                    //Target has IClickListener and it has lower priority than all the event listeners, call it last
                    if (listener != null && !onClickProced && UIManager.Instance.CanFire(listener.MouseEventData.Tags)) {
                        listener.OnClick(Button);
                    }
                }
            }

            public void Add(MouseEvent mouseEvent)
            {
                Events.Add(mouseEvent);
                Events = Events.OrderByDescending(mouseEvent => mouseEvent.Priority).ToList();
            }

            public void Remove(MouseEvent oldEvent)
            {
                Events = Events.Where(mouseEvent => !mouseEvent.Equals(oldEvent)).OrderByDescending(mouseEvent => mouseEvent.Priority).ToList();
            }
        }
    }
}
