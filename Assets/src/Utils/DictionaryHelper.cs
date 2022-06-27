using System;
using System.Collections.Generic;

namespace Game.Utils
{
    public class DictionaryHelper
    {
        public static Dictionary<TEnum, TItem> CreateNewFromEnum<TEnum, TItem>(TItem defaultValue)
        {
            Dictionary<TEnum, TItem> dictionary = new Dictionary<TEnum, TItem>();
            foreach (TEnum e in Enum.GetValues(typeof(TEnum))) {
                dictionary.Add(e, defaultValue);
            }
            return dictionary;
        }

        public static Dictionary<TEnum, TItem> CreateNewFromEnum<TEnum, TItem>(Func<TEnum, TItem> defaultCreatorCallback)
        {
            Dictionary<TEnum, TItem> dictionary = new Dictionary<TEnum, TItem>();
            foreach (TEnum e in Enum.GetValues(typeof(TEnum))) {
                dictionary.Add(e, defaultCreatorCallback(e));
            }
            return dictionary;
        }
    }
}
