using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Utils
{
    public class DictionaryHelper
    {
        /// <summary>
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="defaultValue">Note: This parameter is given as default value for each key, thus using objects is not adviced. For example, if this is a List all keys get reference to same list. Using Func<TEnum, TItem> - parameter is usually a better option in such a case.</param>
        /// <returns></returns>
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
            foreach (KeyValuePair<TKey, TValue> originalPair in original) {
                copy.Add(originalPair.Key, valueCreatorCallback(originalPair.Key, originalPair.Value));
            }
            return copy;
        }

        public static void Set<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key)) {
                dictionary[key] = value;
            } else {
                dictionary.Add(key, value);
            }
        }

        public static void Plus<TKey>(Dictionary<TKey, float> dictionary, TKey key, float amount)
        {
            if (dictionary.ContainsKey(key)) {
                dictionary[key] += amount;
            } else {
                dictionary.Add(key, amount);
            }
        }

        public static void Plus<TKey>(Dictionary<TKey, float> dictionary, float amount)
        {
            List<TKey> keys = dictionary.Keys.Select(key => key).ToList();
            foreach (TKey key in keys) {
                dictionary[key] += amount;
            }
        }

        public static void Minus<TKey>(Dictionary<TKey, float> dictionary, TKey key, float amount)
        {
            if (dictionary.ContainsKey(key)) {
                dictionary[key] -= amount;
            } else {
                dictionary.Add(key, (-1.0f) * amount);
            }
        }

        public static void Minus<TKey>(Dictionary<TKey, float> dictionary, float amount)
        {
            List<TKey> keys = dictionary.Keys.Select(key => key).ToList();
            foreach (TKey key in keys) {
                dictionary[key] -= amount;
            }
        }
    }
}
