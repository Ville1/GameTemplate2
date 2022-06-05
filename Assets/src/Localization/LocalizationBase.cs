using UnityEngine.Localization;

namespace Game.Localization {
    public class LocalizationBase
    {
        protected static string GetString(string tableName, string key)
        {
            LocalizedString localized = new LocalizedString();
            localized.TableReference = tableName;
            localized.TableEntryReference = key;
            return localized.GetLocalizedString();
        }
    }
}
