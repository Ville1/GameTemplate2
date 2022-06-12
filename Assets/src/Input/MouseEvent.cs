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
        public delegate void OnClickDelegate(GameObject target, MouseButton button);

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
}
