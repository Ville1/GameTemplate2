using Game.Utils;

namespace Game.UI
{
    /// <summary>
    /// Template for ui panels
    /// </summary>
    public class NotificationHistoryWindowManager : WindowBase
    {
        public static NotificationHistoryWindowManager Instance;

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
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        {
            base.Update();
        }
    }
}