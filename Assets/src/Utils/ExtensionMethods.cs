using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
