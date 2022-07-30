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

        public static Dictionary<TKey, TValue> Copy<TKey, TValue>(Dictionary<TKey, TValue> original)
        {
            Dictionary<TKey, TValue> copy = new Dictionary<TKey, TValue>();
            foreach (KeyValuePair<TKey, TValue> originalPair in original) {
                copy.Add(originalPair.Key, originalPair.Value);
            }
            return copy;
        }

        public static Dictionary<TKey, TValue> Copy<TKey, TValue>(Dictionary<TKey, TValue> original, Func<TKey, TValue, TValue> valueCreatorCallback)
        {
            Dictionary<TKey, TValue> copy = new Dictionary<TKey, TValue>();
            foreach(KeyValuePair<TKey, TValue> originalPair in original) {
                copy.Add(originalPair.Key, valueCreatorCallback(originalPair.Key, originalPair.Value));
            }
            return copy;
        }
    }
}
