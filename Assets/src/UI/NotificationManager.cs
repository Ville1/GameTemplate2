using Game.Input;
using Game.Saving;
using Game.Saving.Data;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.UI
{
    public class NotificationManager : MonoBehaviour, ISaveable
    {
        public static readonly LString TIME_STAMP_PREFIX = null;//For example "{Turn} "
        public static readonly bool SHOW_TIME_STAMP = true;

        private static readonly float ANIMATE_MOVE_SPEED = 150.0f;//-1.0f = no animation
        private static readonly bool ANIMATE_SLIDE_NEW_CARDS = true;//If true, new cards are placed on the last possible position. Does nothing, if animations are off.
        private static readonly float QUEUE_UPDATE_FREQUENCY = 0.01f;//Seconds

        private static readonly float MARGIN = 0.0f;
        private static readonly int MAX_CARDS = 20;//TODO: Scale with screen size?

        public static NotificationManager Instance;
        public static bool Animate => ANIMATE_MOVE_SPEED > 0.0f;

        public GameObject NotificationPrototype;

        private bool showNotifications = true;
        private List<Notification> notifications = new List<Notification>();
        private List<Notification> addQueue = new List<Notification>();
        private List<Notification> deleteQueue = new List<Notification>();
        private List<Notification> cardAddQueue = new List<Notification>();
        private List<Notification> cardCloseQueue = new List<Notification>();
        private List<NotificationCard> notificationCards = new List<NotificationCard>();
        private float notificationWidth;
        private float updateCooldown = 0.0f;
        private bool isSaving = false;
        private int saveIndex = 0;

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
            if(!isSaving) {
                ProcessQueues();
                if (ShowNotifications) {
                    if (Animate) {
                        foreach (NotificationCard card in notificationCards) {
                            card.Update();
                        }
                    }
                    while (ProcessAddCardQueue()) { };
                }
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
                    updateCooldown = 0.0f;
                }
            }
        }

        public void Add(Notification notification)
        {
            if(!addQueue.Any(added => added.Id == notification.Id)) {
                addQueue.Add(notification);
            }
            deleteQueue = deleteQueue.Where(deleted => deleted.Id != notification.Id).ToList();
        }

        public void Delete(Notification notification)
        {
            if(!deleteQueue.Any(deleted => deleted.Id == notification.Id)) {
                deleteQueue.Add(notification);
            }
            addQueue = addQueue.Where(added => added.Id != notification.Id).ToList();
            cardAddQueue = cardAddQueue.Where(newCard => newCard.Id != notification.Id).ToList();
        }

        public void Close(Notification notification)
        {
            if (!cardCloseQueue.Any(close => close.Id == notification.Id)) {
                cardCloseQueue.Add(notification);
            }
            cardAddQueue = cardAddQueue.Where(newCard => newCard.Id != notification.Id).ToList();
        }

        public void CloseAll()
        {
            foreach(NotificationCard card in notificationCards) {
                Close(card.Notification);
            }
        }

        public void SkipAnimation()
        {
            foreach (NotificationCard card in notificationCards) {
                card.SkipMovement();
            }
        }

        public List<Notification> Notifications
        {
            get {
                return notifications.Select(notification => notification).ToList();
            }
        }

        public float Load(ISaveData data)
        {
            NotificationListData saveData = data as NotificationListData;
            if(saveData.List.Count == 0) {
                EndSaveDataProcessing();
                return 1.0f;
            }

            NotificationData notificationData = saveData.List[saveIndex];
            Notification notification = new Notification(notificationData);
            notifications.Add(notification);
            if (notificationData.HasCard) {
                cardAddQueue.Add(notification);
            }
            saveIndex++;

            if (saveIndex == saveData.List.Count) {
                EndSaveDataProcessing();
                return 1.0f;
            }

            return saveIndex / (float)saveData.List.Count;
        }

        public float Save(ref ISaveData data)
        {
            if (notifications.Count == 0) {
                EndSaveDataProcessing();
                return 1.0f;
            }

            NotificationListData saveData = data as NotificationListData;
            Notification notification = notifications[saveIndex];
            saveIndex++;

            saveData.List.Add(notification.Save(notificationCards.Any(card => card.Notification.Id == notification.Id)));

            if(saveIndex == notifications.Count) {
                EndSaveDataProcessing();
                return 1.0f;
            }

            return saveIndex / (float)notifications.Count;
        }

        public void StartLoading(ISaveData data)
        {
            CloseAll();
            notifications.Clear();
            addQueue.Clear();
            deleteQueue.Clear();
            cardAddQueue.Clear();
            cardCloseQueue.Clear();
            NotificationListData saveData = data as NotificationListData;
            saveData.List = saveData.List ?? new List<NotificationData>();
            StartSaveDataProcessing();
        }

        public void StartSaving(ref ISaveData data)
        {
            (data as NotificationListData).List = new List<NotificationData>();
            StartSaveDataProcessing();
        }

        private void StartSaveDataProcessing()
        {
            isSaving = true;
            saveIndex = 0;
        }

        private void EndSaveDataProcessing()
        {
            isSaving = false;
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
            cardAddQueue = cardAddQueue.Where(queuedNotification => queuedNotification.Id != notification.Id).ToList();
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
                notification.OnClick(notification);
            }
            if (notification.CloseOnClick) {
                CloseNotification(notification);
            }
        }

        private void CloseNotification(Notification notification)
        {
            NotificationCard card = notificationCards.FirstOrDefault(c => c.Notification.Id == notification.Id);
            if (card == null) {
                return;
            }

            if (notification.HasTitle) {
                TooltipManager.Instance.UnregisterTooltip(card.GameObject);
            }
            MouseManager.Instance.RemoveEventListener(MouseButton.Right, card.RightClickEventId);

            Destroy(card.GameObject);

            for (int i = card.Index + 1; i < notificationCards.Count; i++) {
                notificationCards[i].Move(-1);
            }
            notificationCards.Remove(card);
        }

        //TODO: This function could be merged with CreateNotification, by moving checks there and having it return bool. But maybe this looks cleaner?
        private bool ProcessAddCardQueue()
        {
            if (updateCooldown > 0.0f) {
                updateCooldown = Math.Max(updateCooldown - Time.deltaTime, 0.0f);
                //On cooldown
                return false;
            }
            updateCooldown += QUEUE_UPDATE_FREQUENCY;

            if (cardAddQueue.Count == 0) {
                //No queue
                return false;
            }

            Notification nextNotification = cardAddQueue[0];
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

        private void ProcessQueues()
        {
            bool hasChanges = addQueue.Count != 0 || deleteQueue.Count != 0;
            foreach (Notification newNotification in addQueue) {
                cardAddQueue.Add(newNotification);
                notifications.Add(newNotification);
            }
            addQueue.Clear();

            foreach(Notification deleteNotification in deleteQueue) {
                CloseNotification(deleteNotification);
                notifications = notifications.Where(notification => notification.Id != deleteNotification.Id).ToList();
            }
            deleteQueue.Clear();

            foreach(Notification closeNotification in cardCloseQueue) {
                CloseNotification(closeNotification);
            }
            cardCloseQueue.Clear();

            if (hasChanges && NotificationHistoryWindowManager.Instance.Active) {
                NotificationHistoryWindowManager.Instance.UpdateUI();
            }
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

        public delegate void NotificationCallback(Notification notification);

        public Guid Id { get; private set; }
        public NotificationType Type { get; private set; }
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
        public object NotificationSpecificData { get; private set; }

        public Notification(NotificationType type, LString title, LString description, string timeStamp, UISpriteData imageData, string cardText = null, bool? closeOnClick = null, object notificationSpecificData = null, NotificationCallback onClick = null)
        {
            Initialize(type, title, description, timeStamp, imageData, cardText, closeOnClick, notificationSpecificData, onClick, null);
        }

        public Notification(NotificationType type, LString title, LString description, UISpriteData imageData, object notificationSpecificData = null, NotificationCallback onClick = null)
        {
            Initialize(type, title, description, null, imageData, null, null, notificationSpecificData, onClick, null);
        }

        public Notification(NotificationType type, LString title, LString description, string cardText, object notificationSpecificData = null, NotificationCallback onClick = null)
        {
            Initialize(type, title, description, null, null, cardText, null, notificationSpecificData, onClick, null);
        }

        public Notification(NotificationType type, LString title, LString description, object notificationSpecificData = null, NotificationCallback onClick = null)
        {
            Initialize(type, title, description, null, null, null, null, notificationSpecificData, onClick, null);
        }

        public Notification(NotificationData saveData)
        {
            Initialize((NotificationType)saveData.Type, saveData.Title, saveData.Description, saveData.TimeStamp, null, null, null, null, null, Guid.Parse(saveData.Id));
            Load(saveData);
        }

        private void Initialize(NotificationType type, LString title, LString description, string timeStamp, UISpriteData imageData, string cardText, bool? closeOnClick,
            object notificationSpecificData, NotificationCallback onClick, Guid? id)
        {
            Id = id.HasValue ? id.Value : Guid.NewGuid();
            Type = type;
            Title = title;
            Description = description;
            TimeStamp = timeStamp;
            ImageData = imageData ?? new UISpriteData();
            CardText = cardText;
            CloseOnClick = closeOnClick.HasValue ? closeOnClick.Value : DEFAULT_CLOSE_ON_CLICK;
            NotificationSpecificData = notificationSpecificData;
            OnClick = onClick;

            if(ImageData.IsEmpty && string.IsNullOrEmpty(CardText)) {
                CardText = DEFAULT_NO_IMAGE_CARD_TEXT;
            }
        }

        /// <summary>
        /// TODO: Implement real NotificationTypes here in a real game
        /// </summary>
        private void Load(NotificationData saveData)
        {
            switch (Type) {
                case NotificationType.TestType:
                    string[] split = saveData.Data.Split(";");
                    NotificationSpecificData = Main.Instance.WorldMap.GetTileAt(int.Parse(split[0]), int.Parse(split[1]));
                    OnClick = (Notification notification) => { CameraManager.Instance.Center(NotificationSpecificData as Maps.Tile); };
                    ImageData = new UISpriteData("horn/stick figure horn 4", TextureDirectory.Sprites);
                    break;
            }
        }

        /// <summary>
        /// TODO: Implement real NotificationTypes here in a real game
        /// </summary>
        public NotificationData Save(bool hasCard)
        {
            NotificationData data = new NotificationData() {
                Id = Id.ToString(),
                Type = (int)Type,
                Title = Title.IsLocalized ? string.Format("{{0}}", Title.Key) : Title,
                Description = Description.IsLocalized ? string.Format("{{0}}", Description.Key) : Description,
                TimeStamp = TimeStamp,
                Data = null,
                HasCard = hasCard
            };
            switch (Type) {
                case NotificationType.TestType:
                    data.Data = (NotificationSpecificData as Maps.Tile).X + ";" + (NotificationSpecificData as Maps.Tile).Y;
                    break;
            }
            return data;
        }
    }
}
