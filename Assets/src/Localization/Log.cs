namespace Game.Localization
{
    /// <summary>
    /// Localization for logging
    /// </summary>
    public class Log : LocalizationBase
    {
        /// <summary>
        /// Get localized string by key
        /// </summary>
        public static string Get(string key)
        {
            return GetString("Log", key);
        }
    }
}