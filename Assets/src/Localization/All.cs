namespace Game.Localization
{
    /// <summary>
    /// All localization
    /// </summary>
    public class All : LocalizationBase
    {
        /// <summary>
        /// Get localized string by table and key
        /// </summary>
        public static string Get(string table, string key)
        {
            return GetString(table, key);
        }
    }
}