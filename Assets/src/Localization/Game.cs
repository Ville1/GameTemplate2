namespace Game.Localization
{
    /// <summary>
    /// In game localization
    /// </summary>
    public class Game : LocalizationBase
    {
        /// <summary>
        /// Get localized string by key
        /// </summary>
        public static string Get(string key)
        {
            return GetString("Game", key);
        }
    }
}