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
    }
}
