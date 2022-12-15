using System;
using System.Collections.Generic;

namespace Game.Saving.Data
{
    [Serializable]
    public class NotificationListData : ISaveData
    {
        public List<NotificationData> List;
    }

    [Serializable]
    public class NotificationData : ISaveData
    {
        public string Id;
        public int Type;
        public string Title;
        public string Description;
        public string TimeStamp;
        public string Data;
        public bool HasCard;
    }
}
