using Game.Utils;
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

        public Guid Id { get; private set; }
        public GameObject Target { get; private set; }
        public OnClickDelegate Listener { get; private set; }
        public int Priority { get { return EventData == null ? 0 : EventData.Priority; } }
        public List<MouseEventTag> Tags { get { return EventData == null ? null : EventData.Tags; } }
        /// <summary>
        /// TODO: This and MouseEventTag.IgnoreUI are both used for similar stuff, maybe one of these needs to be removed
        /// </summary>
        public bool IsBlockedByUI { get { return EventData == null ? true : EventData.IsBlockedByUI; } }
        public MouseEventData EventData { get; private set; }

        public MouseEvent(IClickListener target, OnClickDelegate listener, int priority = 0, List<MouseEventTag> tags = null, bool isBlockedByUI = true)
        {
            if (target == null || listener == null) {
                throw new NullReferenceException();
            }
            Initialize(target.GameObject, listener, priority, tags, isBlockedByUI);
        }

        public MouseEvent(GameObject target, OnClickDelegate listener, int priority = 0, List<MouseEventTag> tags = null, bool isBlockedByUI = true)
        {
            Initialize(target, listener, priority, tags, isBlockedByUI);
        }

        public MouseEvent(OnClickDelegate listener, int priority = 0, List<MouseEventTag> tags = null, bool isBlockedByUI = true)
        {
            Initialize(null, listener, priority, tags, isBlockedByUI);
        }

        public MouseEvent(OnClickDelegate listener, int priority, MouseEventTag tag, bool isBlockedByUI = true)
        {
            Initialize(null, listener, priority, new List<MouseEventTag>() { tag }, isBlockedByUI);
        }

        private void Initialize(GameObject target, OnClickDelegate listener, int priority = 0, List<MouseEventTag> tags = null, bool isBlockedByUI = true)
        {
            Id = Guid.NewGuid();
            Target = target;
            Listener = listener;
            EventData = new MouseEventData(priority, tags, isBlockedByUI);
        }
    }

    public class MouseEventData
    {
        public int Priority { get; set; }
        public List<MouseEventTag> Tags { get; set; }
        /// <summary>
        /// Stops event from firing, if there is any ui elements under cursor
        /// </summary>
        public bool IsBlockedByUI { get; private set; }

        public MouseEventData(int priority, List<MouseEventTag> tags, bool isBlockedByUI)
        {
            Priority = priority;
            Tags = tags ?? new List<MouseEventTag>();
            IsBlockedByUI = isBlockedByUI;
        }

        public static MouseEventData Default
        {
            get { return new MouseEventData(0, new List<MouseEventTag>(), true); }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is MouseEventData)) {
                return false;
            }
            MouseEventData otherData = obj as MouseEventData;
            return Priority == otherData.Priority && Tags.HasSameItems(otherData.Tags) && IsBlockedByUI == otherData.IsBlockedByUI;
        }
    }

    public class MouseNothingClickEvent
    {
        public delegate void OnClickDelegate();

        public Guid Id { get; private set; }
        public OnClickDelegate Listener { get; private set; }
        public int Priority { get { return EventData == null ? 0 : EventData.Priority; } }
        public List<MouseEventTag> Tags { get { return EventData == null ? null : EventData.Tags; } }
        public bool IsBlockedByUI { get { return EventData == null ? true : EventData.IsBlockedByUI; } }
        public MouseEventData EventData { get; private set; }

        public MouseNothingClickEvent(OnClickDelegate listener, int priority = 0, List<MouseEventTag> tags = null, bool isBlockedByUI = true)
        {
            if (listener == null) {
                throw new NullReferenceException();
            }
            Id = Guid.NewGuid();
            Listener = listener;
            EventData = new MouseEventData(priority, tags, isBlockedByUI);
        }

        public MouseNothingClickEvent(OnClickDelegate listener, int priority, MouseEventTag tag, bool isBlockedByUI = true)
        {
            if (listener == null) {
                throw new NullReferenceException();
            }
            Id = Guid.NewGuid();
            Listener = listener;
            EventData = new MouseEventData(priority, new List<MouseEventTag>() { tag }, isBlockedByUI);
        }
    }

    public class MouseDragEvent
    {
        public enum TargetType { NoTarget, GameObject, ClickListener }

        public delegate void OnDragDelegateNoTarget(Vector3 vector);//vector = mouse position if drag start/end or delta if move (WORLD POSITION)
        public delegate void OnDragDelegateGameObject(Vector3 vector, GameObject draggedObject, GameObject targetObject);
        public delegate void OnDragDelegateClickable(Vector3 vector, IClickListener draggedObject, IClickListener targetObject);

        public Guid Id { get; private set; }
        public GameObject GameObjectTarget { get; private set; } = null;
        public IClickListener ClickableTarget { get; private set; } = null;
        public OnDragDelegateNoTarget TargetlessListener { get; private set; } = null;
        public OnDragDelegateGameObject GameObjectListener { get; private set; } = null;
        public OnDragDelegateClickable ClickableListener { get; private set; } = null;
        public int Priority { get { return EventData == null ? 0 : EventData.Priority; } }
        public List<MouseEventTag> Tags { get { return EventData == null ? null : EventData.Tags; } }
        public bool IsBlockedByUI { get { return EventData == null ? true : EventData.IsBlockedByUI; } }
        public MouseEventData EventData { get; private set; }
        public TargetType Targeting { get { return GameObjectTarget == null ? TargetType.NoTarget : (ClickableTarget == null ? TargetType.GameObject : TargetType.ClickListener); } }

        public MouseDragEvent(IClickListener target, OnDragDelegateClickable listener, int priority = 0, List<MouseEventTag> tags = null, bool isBlockedByUI = true)
        {
            if (target == null || listener == null) {
                throw new NullReferenceException();
            }
            Id = Guid.NewGuid();
            GameObjectTarget = target.GameObject;
            ClickableTarget = target;
            ClickableListener = listener;
            EventData = new MouseEventData(priority, tags, isBlockedByUI);
        }

        public MouseDragEvent(GameObject target, OnDragDelegateGameObject listener, int priority = 0, List<MouseEventTag> tags = null, bool isBlockedByUI = true)
        {
            if (target == null || listener == null) {
                throw new NullReferenceException();
            }
            Id = Guid.NewGuid();
            GameObjectTarget = target;
            GameObjectListener = listener;
            EventData = new MouseEventData(priority, tags, isBlockedByUI);
        }

        public MouseDragEvent(OnDragDelegateNoTarget listener, int priority = 0, List<MouseEventTag> tags = null, bool isBlockedByUI = true)
        {
            if (listener == null) {
                throw new NullReferenceException();
            }
            Id = Guid.NewGuid();
            TargetlessListener = listener;
            EventData = new MouseEventData(priority, tags, isBlockedByUI);
        }
    }
}
