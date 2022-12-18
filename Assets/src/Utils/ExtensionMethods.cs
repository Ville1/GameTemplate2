using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Game.Utils
{
    public static class ExtensionMethods
    {
        public static string Capitalize(this string str)
        {
            if (str == string.Empty) {
                return string.Empty;
            }
            if(str.Length == 1) {
                return str.ToUpper();
            }
            return str[0].ToString().ToUpper() + str.Substring(1);
        }

        public static List<string> Replicate(this string format, int count)
        {
            return format.Replicate(0, count);
        }

        public static List<string> Replicate(this string format, int startIndex, int count)
        {
            List<string> list = new List<string>();
            if(count == 0) {
                return list;
            }

            if(count > 0) {
                for (int i = startIndex; i < count + startIndex; i++) {
                    list.Add(string.Format(format, i));
                }
            } else {
                for (int i = startIndex; i > count + startIndex; i--) {
                    list.Add(string.Format(format, i));
                }
            }

            return list;
        }

        public static string ToPercentage(this float number)
        {
            return Math.Round((double)number * 100.0f) + "%";
        }

        public static string ToPercentage(this double number)
        {
            return Math.Round(number * 100.0d) + "%";
        }

        public static string ToPercentage(this decimal number)
        {
            return Math.Round(number * 100.0m) + "%";
        }

        public static string Parse(this float number, int digits, bool showZeros = false, bool showPlusSign = false)
        {
            return ((double)number).Parse(digits, showZeros, showPlusSign);
        }

        public static string Parse(this double number, int digits, bool showZeros = false, bool showPlusSign = false)
        {
            if(digits < 0) {
                throw new ArgumentException("Negative digit count");
            }
            double rounded = Math.Round(number, digits);
            string roundedString = rounded.ToString();

            if (showZeros && digits > 0) {
                StringBuilder builder = new StringBuilder(roundedString);
                int currentDigits = 0;
                if (!roundedString.Contains(".")) {
                    builder.Append(".0");
                    currentDigits = 1;
                } else {
                    currentDigits = roundedString.Substring(roundedString.IndexOf(".")).Length - 1;
                }
                while (currentDigits < digits) {
                    builder.Append("0");
                    currentDigits++;
                }
                roundedString = builder.ToString();
            }

            return string.Format("{0}{1}", (showPlusSign && number >= 0.0d ? "+" : string.Empty), roundedString);
        }

        public static List<TItem> Copy<TItem>(this List<TItem> list)
        {
            return list.Select(item => item).ToList();
        }

        /// <summary>
        /// Returns true, if both lists contain each other's items. Order and duplicates are ignored.
        /// </summary>
        public static bool HasSameItems<TItem>(this List<TItem> thisList, List<TItem> list)
        {
            foreach(TItem item in thisList) {
                if (!list.Contains(item)) {
                    return false;
                }
            }
            foreach (TItem item in list) {
                if (!thisList.Contains(item)) {
                    return false;
                }
            }
            return true;
        }

        public static TEnum Shift<TEnum>(this TEnum e, int amount) where TEnum : struct, IConvertible
        {
            if (!typeof(TEnum).IsEnum) {
                throw new ArgumentException("TEnum must be an enum type");
            }
            List<TEnum> values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
            int originalIndex = -1;
            for (int i = 0; i < values.Count && originalIndex == -1; i++) {
                if (values[i].Equals(e)) {
                    originalIndex = i;
                }
            }
            if(originalIndex == -1) {
                //This should not be possible
                throw new ArgumentException("TEnum does not contain e");
            }
            int newIndex = originalIndex + amount;
            while(newIndex >= values.Count) {
                newIndex -= values.Count;
            }
            while(newIndex < 0) {
                newIndex += values.Count;
            }
            return values[newIndex];
        }

        public static Vector2 Clone(this Vector2 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        public static Vector3 Clone(this Vector3 vector)
        {
            return new Vector3(vector.x, vector.y, vector.z);
        }
    }
}
