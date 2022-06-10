using Game.Utils;

namespace Game.UI
{
    /// <summary>
    /// Template for ui panels
    /// </summary>
    public class Template : WindowBase
    {
        public static Template Instance;

        /// <summary>
        /// Initializiation
        /// </summary>
        protected override void Start()
        {
            if (Instance != null) {
                CustomLogger.Error("AttemptingToCreateMultipleInstances");
                return;
            }
            Instance = this;
            //Tags.Add(Tag.ProgressBar);
        }

        /// <summary>
        /// Per frame update
        /// </summary>
        protected override void Update()
        { }
    }
}