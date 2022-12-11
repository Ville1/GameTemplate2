using Game.Input;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.UI
{
    public class NotificationManager : MonoBehaviour
    {
        private static readonly LString TIME_STAMP_PREFIX = null;//For example "{Turn}"
        private static readonly bool SHOW_TIME_STAMP = true;

        private static readonly float ANIMATE_MOVE_SPEED = 150.0f;//-1.0f = no animation
        private static readonly bool ANIMATE_SLIDE_NEW_CARDS = true;//If true, new cards are placed on the last possible position. Does nothing, if animations are off.
        private static readonly float QUEUE_UPDATE_FREQUENCY = 0.01f;//Seconds

        private static readonly float MARGIN = 0.0f;
        private static readonly int MAX_CARDS = 20;//TODO: Scale with screen size?

        public static NotificationManager Instance;
        public static bool Animate => ANIMATE_MOVE_SPEED > 0.0f;

        public GameObject NotificationPrototype;

        private bool showNotifications = true;
        private List<Notification> notificationsAddedWhenNotVisible = new List<Notification>();
        private List<Notification> notificationQueue = new List<Notification>();
        private List<Notification> notifications = new List<Notification>();
        private List<NotificationCard> notificationCards = new List<NotificationCard>();
        private float notificationWidth;
        private float updateCooldown = 0.0f;

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

            NotificationPrototype.SetActive(false);
            notificationWidth = NotificationPrototype.GetComponent<RectTransform>().rect.width;

            KeyboardManager.Instance.AddOnKeyDownEventListener(KeyBindings.CloseAllNotifications, CloseAll);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        private void Update()
        {
            if(ShowNotifications && Animate) {
                if (Animate) {
                    foreach (NotificationCard card in notificationCards) {
                        card.Update();
                    }
                }
                while (ProcessQueue()) { };
            }
        }

        public bool ShowNotifications
        {
            get {
                return showNotifications;
            }
            set {
                bool oldValue = showNotifications;
                showNotifications = value;
                CardsSetActive(showNotifications);
                if(!oldValue && showNotifications) {
                    foreach(Notification notification in notificationsAddedWhenNotVisible) {
                        notificationQueue.Add(notification);
                    }
                    notificationsAddedWhenNotVisible.Clear();
                    updateCooldown = 0.0f;
                }
            }
        }

        public void Add(Notification notification)
        {
            if (ShowNotifications) {
                notificationQueue.Add(notification);
            } else {
                notificationsAddedWhenNotVisible.Add(notification);
            }
        }

        public void CloseAll()
        {
            foreach(NotificationCard card in notificationCards) {
                CloseNotification(card.Notification, true);
            }
            notificationCards.Clear();
        }

        public void SkipAnimation()
        {
            foreach (NotificationCard card in notificationCards) {
                card.SkipMovement();
            }
        }

        private void CreateNotification(Notification notification)
        {
            //Instantiate a new card GameObject
            //Calculate card position

            NotificationCard lastCard = LastCard;
            Vector3 targetPosition = new Vector3(
                NotificationPrototype.transform.position.x + notificationCards.Count * (notificationWidth + MARGIN),
                NotificationPrototype.transform.position.y,
                NotificationPrototype.transform.position.z
            );
            Vector3 currentPosition = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);

            if (ANIMATE_SLIDE_NEW_CARDS) {
                //Place new card in rightmost possible position
                currentPosition = new Vector3(
                    NotificationPrototype.transform.position.x + (MAX_CARDS - 1) * (notificationWidth + MARGIN),
                    NotificationPrototype.transform.position.y,
                    NotificationPrototype.transform.position.z
                );
            } else if(Animate && lastCard != null && lastCard.IsMoving) {
                //Last card is still moving, and this card needs to go on it's right side
                currentPosition = new Vector3(
                    lastCard.Position.x + notificationWidth + MARGIN,
                    lastCard.Position.y,
                    lastCard.Position.z
                );
            }

            GameObject card = Instantiate(
                NotificationPrototype,
                currentPosition,
                Quaternion.identity,
                gameObject.transform
            );
            card.name = string.Format("Notification {0}", notification.Id);
            card.SetActive(true);

            //Update card
            UIHelper.SetButton(card, "Hidden Button", null, () => { NotificationClick(notification); });
            UIHelper.SetText(card, "Text", notification.CardText);
            UIHelper.SetImage(card, "Image", notification.ImageData);

            if (notification.HasTitle) {
                //Add tooltip
                TooltipManager.Instance.RegisterTooltip(new Tooltip(card, notification.Title));
            }

            //Add right click listener
            Guid clickEventId = MouseManager.Instance.AddEventListener(MouseButton.Right, new MouseEvent(card, (GameObject target) => { CloseNotification(notification); }));

            NotificationCard notificationCard = new NotificationCard(notification, card, clickEventId, notificationCards.Count, notificationWidth);

            if (targetPosition.x != currentPosition.x) {
                //Card needs to start moving to right position
                notificationCard.SetMovementTarget(targetPosition);
            }

            //Add to lists
            notificationCards.Add(notificationCard);
            notifications.Add(notification);

            if (notificationQueue.Contains(notification)) {
                notificationQueue.Remove(notification);
            }
        }

        private void CardsSetActive(bool active)
        {
            foreach(NotificationCard card in notificationCards) {
                card.GameObject.SetActive(active);
            }
        }

        private void NotificationClick(Notification notification)
        {
            if(notification.OnClick != null) {
                notification.OnClick();
            }
            if (notification.CloseOnClick) {
                CloseNotification(notification);
            }
        }

        private void CloseNotification(Notification notification, bool closingAllNotifications = false)
        {
            NotificationCard card = notificationCards.FirstOrDefault(c => c.Notification.Id == notification.Id);
            if (card == null) {
                CustomLogger.Warning("{NotificationHasNoCard}", notification.Id);
                return;
            }

            if (notification.HasTitle) {
                TooltipManager.Instance.UnregisterTooltip(card.GameObject);
            }
            MouseManager.Instance.RemoveEventListener(MouseButton.Right, card.RightClickEventId);

            Destroy(card.GameObject);

            if (!closingAllNotifications) {
                for (int i = card.Index + 1; i < notificationCards.Count; i++) {
                    notificationCards[i].Move(-1);
                }
                notificationCards.Remove(card);
            }
        }

        private bool ProcessQueue()
        {
            if (updateCooldown > 0.0f) {
                updateCooldown = Math.Max(updateCooldown - Time.deltaTime, 0.0f);
                //On cooldown
                return false;
            }
            updateCooldown += QUEUE_UPDATE_FREQUENCY;

            if (notificationQueue.Count == 0) {
                //No queue
                return false;
            }

            Notification nextNotification = notificationQueue[0];
            if (notificationCards.Count == 0) {
                //No pre-existing cards
                CreateNotification(nextNotification);
                return true;
            }

            if(notificationCards.Count >= MAX_CARDS) {
                //Max capacity
                return false;
            }

            if (!ANIMATE_SLIDE_NEW_CARDS) {
                //New card is just placed next to last one's position and since card limit has not been reached, there must be space for it
                CreateNotification(nextNotification);
                return true;
            }

            //Check if there is enough space for the new card
            NotificationCard lastCard = LastCard;

            if (!lastCard.IsMoving) {
                //Last card is in it's final position
                CreateNotification(nextNotification);
                return true;
            }

            float lastCardRightBorderPosition = lastCard.Position.x + notificationWidth;
            float newCardLeftBorderPosition = NotificationPrototype.transform.position.x + (MAX_CARDS - 1) * (notificationWidth + MARGIN);
            if(lastCardRightBorderPosition > newCardLeftBorderPosition) {
                //Not enough space
                return false;
            }

            CreateNotification(nextNotification);
            return true;
        }

        private NotificationCard LastCard {
            get {
                return notificationCards.Count == 0 ? null : notificationCards[notificationCards.Count - 1];
            }
        }

        private class NotificationCard
        {
            public Notification Notification { get; private set; }
            public GameObject GameObject { get; private set; }
            public Guid RightClickEventId { get; private set; }
            public int Index { get; private set; }
            public float Width { get; private set; }
            public bool IsMoving { get { return targetPosition.HasValue; } }

            private Vector3? oldPosition = null;
            private Vector3? targetPosition = null;
            private float movementDistanceTotal = -1.0f;
            private float movementDistanceCurrent = -1.0f;

            public NotificationCard(Notification notification, GameObject gameObject, Guid rightClickEventId, int index, float width)
            {
                Notification = notification;
                GameObject = gameObject;
                RightClickEventId = rightClickEventId;
                Index = index;
                Width = width;
            }

            public void Move(int delta)
            {
                if (Animate) {
                    SetMovementTarget(GetNewPosition(IsMoving ? targetPosition.Value : Position, delta));
                } else {
                    Position = GetNewPosition(Position, delta);
                }
                Index += delta;
            }

            public void SetMovementTarget(Vector3 target)
            {
                oldPosition = Position;
                targetPosition = target;
                movementDistanceTotal = Vector3.Distance(oldPosition.Value, targetPosition.Value);
                movementDistanceCurrent = 0.0f;
            }

            public void SkipMovement()
            {
                if (IsMoving) {
                    Position = targetPosition.Value;
                    EndMovement();
                }
            }

            public void Update()
            {
                if (!IsMoving) {
                    return;
                }
                movementDistanceCurrent = Mathf.Clamp(movementDistanceCurrent + Time.deltaTime * ANIMATE_MOVE_SPEED, 0.0f, movementDistanceTotal);
                float progress = movementDistanceCurrent / movementDistanceTotal;
                Position = Vector3.Lerp(oldPosition.Value, targetPosition.Value, progress);
                if (progress == 1.0f) {
                    EndMovement();
                }
            }

            public Vector3 Position
            {
                get {
                    return new Vector3(GameObject.transform.position.x, GameObject.transform.position.y, GameObject.transform.position.z);
                }
                set {
                    GameObject.transform.position = new Vector3(value.x, value.y, value.z);
                }
            }

            private Vector3 GetNewPosition(Vector3 current, int indexDelta)
            {
                return new Vector3(current.x + indexDelta * (Width + MARGIN), current.y, current.z);
            }

            private void EndMovement()
            {
                oldPosition = null;
                targetPosition = null;
                movementDistanceTotal = -1.0f;
                movementDistanceCurrent = -1.0f;
            }
        }
    }

    public class Notification
    {
        private static readonly string DEFAULT_NO_IMAGE_CARD_TEXT = "!";
        private static readonly bool DEFAULT_CLOSE_ON_CLICK = false;

        public delegate void NotificationCallback();

        public Guid Id { get; private set; }
        public LString Title { get; private set; }
        public bool HasTitle { get { return !string.IsNullOrEmpty(Title); } }
        public LString Description { get; private set; }
        public bool HasDescription { get { return !string.IsNullOrEmpty(Description); } }
        /// <summary>
        /// In game time stamp, optional. For example, in a turn based game this could be the turn number.
        /// </summary>
        public string TimeStamp { get; private set; }
        public UISpriteData ImageData { get; private set; }
        public string CardText { get; private set; }
        public NotificationCallback OnClick { get; private set; }
        public bool CloseOnClick { get; private set; }

        public Notification(LString title, LString description, string timeStamp, UISpriteData imageData, string cardText = null, bool? closeOnClick = null, NotificationCallback onClick = null)
        {
            Initialize(title, description, timeStamp, imageData, cardText, closeOnClick, onClick);
        }

        public Notification(LString title, LString description, UISpriteData imageData, NotificationCallback onClick = null)
        {
            Initialize(title, description, null, imageData, null, null, onClick);
        }

        public Notification(LString title, LString description, string cardText, NotificationCallback onClick = null)
        {
            Initialize(title, description, null, null, cardText, null, onClick);
        }

        public Notification(LString title, LString description, NotificationCallback onClick = null)
        {
            Initialize(title, description, null, null, null, null, onClick);
        }

        private void Initialize(LString title, LString description, string timeStamp, UISpriteData imageData, string cardText, bool? closeOnClick, NotificationCallback onClick)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            TimeStamp = timeStamp;
            ImageData = imageData ?? new UISpriteData();
            CardText = cardText;
            CloseOnClick = closeOnClick.HasValue ? closeOnClick.Value : DEFAULT_CLOSE_ON_CLICK;
            OnClick = onClick;

            if(ImageData.IsEmpty && string.IsNullOrEmpty(CardText)) {
                CardText = DEFAULT_NO_IMAGE_CARD_TEXT;
            }
        }
    }
}
