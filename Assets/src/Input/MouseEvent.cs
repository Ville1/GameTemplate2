using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Input
{
    public interface IClickListener
    {
        void OnClick(MouseButton button);
        MouseEventData MouseEventData { get; }
        GameObject GameObject { get; }
    }

    public interface IClickListenerComponent
    {
        IClickListener Listener { get; }
    }

    public class MouseEvent
    {
        public delegate void OnClickDelegate(GameObject target);

        public GameObject Target { get; private set; }
        public OnClickDelegate Listener { get; private set; }
        public int Priority { get { return EventData == null ? 0 : EventData.Priority; } }
        public List<MouseEventTag> Tags { get { return EventData == null ? null : EventData.Tags; } }
        public MouseEventData EventData { get; private set; }

        public MouseEvent(IClickListener target, OnClickDelegate listener, int priority = 0, List<MouseEventTag> tags = null)
        {
            if (target == null || listener == null) {
                throw new NullReferenceException();
            }
            Target = target.GameObject;
            Listener = listener;
            EventData = new MouseEventData(priority, tags);
        }

        public MouseEvent(GameObject target, OnClickDelegate listener, int priority = 0, List<MouseEventTag> tags = null)
        {
            if(target == null || listener == null) {
                throw new NullReferenceException();
            }
            Target = target;
            Listener = listener;
            EventData = new MouseEventData(priority, tags);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(!(obj is MouseEvent)) {
                return false;
            }
            MouseEvent otherEvent = obj as MouseEvent;
            return Target == otherEvent.Target && Listener == otherEvent.Listener;
        }
    }

    public class MouseEventData
    {
        public int Priority { get; set; }
        public List<MouseEventTag> Tags { get; set; }

        public MouseEventData(int priority, List<MouseEventTag> tags)
        {
            Priority = priority;
            Tags = tags ?? new List<MouseEventTag>();
        }

        public static MouseEventData Default
        {
            get { return new MouseEventData(0, new List<MouseEventTag>()); }
        }
    }

    public class MouseNothingClickEvent
    {
        public delegate void OnClickDelegate();

        public OnClickDelegate Listener { get; private set; }
        public int Priority { get { return EventData == null ? 0 : EventData.Priority; } }
        public List<MouseEventTag> Tags { get { return EventData == null ? null : EventData.Tags; } }
        public MouseEventData EventData { get; private set; }

        public MouseNothingClickEvent(OnClickDelegate listener, int priority = 0, List<MouseEventTag> tags = null)
        {
            if (listener == null) {
                throw new NullReferenceException();
            }
            Listener = listener;
            EventData = new MouseEventData(priority, tags);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MouseNothingClickEvent)) {
                return false;
            }
            MouseNothingClickEvent otherEvent = obj as MouseNothingClickEvent;
            return Listener == otherEvent.Listener;
        }
    }

    public class MouseDragEvent
    {
        public enum TargetType { NoTarget, GameObject, ClickListener }

        public delegate void OnDragDelegateNoTarget(Vector3 vector);//vector = mouse position if drag start/end or delta if move (WORLD POSITION)
        public delegate void OnDragDelegateGameObject(Vector3 vector, GameObject draggedObject, GameObject targetObject);
        public delegate void OnDragDelegateClickable(Vector3 vector, IClickListener draggedObject, IClickListener targetObject);

        public GameObject GameObjectTarget { get; private set; } = null;
        public IClickListener ClickableTarget { get; private set; } = null;
        public OnDragDelegateNoTarget TargetlessListener { get; private set; } = null;
        public OnDragDelegateGameObject GameObjectListener { get; private set; } = null;
        public OnDragDelegateClickable ClickableListener { get; private set; } = null;
        public int Priority { get { return EventData == null ? 0 : EventData.Priority; } }
        public List<MouseEventTag> Tags { get { return EventData == null ? null : EventData.Tags; } }
        public MouseEventData EventData { get; private set; }
        public TargetType Targeting { get { return GameObjectTarget == null ? TargetType.NoTarget : (ClickableTarget == null ? TargetType.GameObject : TargetType.ClickListener); } }


        public MouseDragEvent(IClickListener target, OnDragDelegateClickable listener, int priority = 0, List<MouseEventTag> tags = null)
        {
            if (target == null || listener == null) {
                throw new NullReferenceException();
            }
            GameObjectTarget = target.GameObject;
            ClickableTarget = target;
            ClickableListener = listener;
            EventData = new MouseEventData(priority, tags);
        }

        public MouseDragEvent(GameObject target, OnDragDelegateGameObject listener, int priority = 0, List<MouseEventTag> tags = null)
        {
            if (target == null || listener == null) {
                throw new NullReferenceException();
            }
            GameObjectTarget = target;
            GameObjectListener = listener;
            EventData = new MouseEventData(priority, tags);
        }

        public MouseDragEvent(OnDragDelegateNoTarget listener, int priority = 0, List<MouseEventTag> tags = null)
        {
            if (listener == null) {
                throw new NullReferenceException();
            }
            TargetlessListener = listener;
            EventData = new MouseEventData(priority, tags);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MouseDragEvent)) {
                return false;
            }
            MouseDragEvent otherEvent = obj as MouseDragEvent;
            return ClickableListener == otherEvent.ClickableListener;
        }
    }
}
