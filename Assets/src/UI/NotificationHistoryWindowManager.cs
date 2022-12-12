using Game.Input;
using Game.UI.Components;
using Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class NotificationHistoryWindowManager : WindowBase
    {
        public static NotificationHistoryWindowManager Instance;

        public Button CloseButton;
        public GameObject ScrollView;
        public TMP_InputField SearchInputField;
        public Button Close2Button;

        private ScrollableList list = null;
        private CustomInputField searchInputField = null;

        /// <summary>
        /// Initializiation
        /// </summary>
        protected override void Start()
        {
            base.Start();
            if (Instance != null) {
                CustomLogger.Error("{AttemptingToCreateMultipleInstances}");
                return;
            }
            Instance = this;

            list = new ScrollableList(ScrollView, null, ScrollRect.ScrollbarVisibility.Permanent, null);
            new CustomButton(CloseButton, null, Close);
            new CustomButton(Close2Button, "{Close}", Close);
            searchInputField = new CustomInputField(SearchInputField, "{FilterNotifications}", HandleSearchInputChange);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        {
            base.Update();
        }

        public override void UpdateUI()
        {
            base.UpdateUI();
            list.Clear();

            List<Notification> notifications = NotificationManager.Instance.Notifications.Where(notification =>
                string.IsNullOrEmpty(searchInputField.Text) ||
                (notification.TimeStamp != null && NotificationManager.SHOW_TIME_STAMP && (
                    notification.TimeStamp.ToLower() == searchInputField.Text.ToLower() ||
                    (!string.IsNullOrEmpty(NotificationManager.TIME_STAMP_PREFIX) && (NotificationManager.TIME_STAMP_PREFIX + notification.TimeStamp).ToLower() == searchInputField.Text.ToLower())
                )) ||
                (notification.HasTitle && notification.Title.ToString().ToLower().Contains(searchInputField.Text.ToLower())) ||
                (notification.HasDescription && notification.Description.ToString().ToLower().Contains(searchInputField.Text.ToLower()))).ToList();

            foreach(Notification notification in notifications) {
                list.AddRow(notification.Id, new List<UIElementData>() {
                    UIElementData.Text("Title Text", (notification.ImageData.IsEmpty ? string.Empty : "      ") + (notification.HasTitle ? notification.Title : "{Notification}"), null),
                    UIElementData.Text("Time Stamp Text", NotificationManager.SHOW_TIME_STAMP ? notification.TimeStamp : null, null),
                    UIElementData.Image("Image", notification.ImageData),
                    UIElementData.Button("Hidden Button", null, null, () => { HandleNotificationClick(notification); }),
                    UIElementData.Button("Delete Button", "X", null, () => { HandleNotificationDeleteClick(notification); }),
                    UIElementData.Tooltip("Hidden Button", notification.Description)
                });
            }
        }

        private void HandleSearchInputChange(string inputText)
        {
            UpdateUI();
        }

        private void HandleNotificationClick(Notification notification)
        {
            if(notification.OnClick != null) {
                notification.OnClick();
            }
        }

        private void HandleNotificationDeleteClick(Notification notification)
        {
            NotificationManager.Instance.Delete(notification);
        }

        private void Close()
        {
            Active = false;
        }
    }
}