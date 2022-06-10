using System;

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
    }
}
