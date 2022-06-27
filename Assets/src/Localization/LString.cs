//Note: keep this in root namespace for ease of use
namespace Game
{
    public class LString
    {
        public static readonly string IMPLICIT_LOCALIZATION_PREFIX = "{";
        public static readonly string IMPLICIT_LOCALIZATION_SUFFIX = "}";
        public static readonly string IMPLICIT_LOCALIZATION_DEFAULT_TABLE = "Game";

        public object[] Arguments { get; private set; } = null;
        public bool IsLocalized { get; private set; } = false;
        public bool IsImplicit { get; private set; } = false;
        private string key = null;
        private string table = null;
        private string text = null;

        /// <summary>
        /// Initializes a new localized string using string.Format.
        /// </summary>
        public LString(string key, string table, params object[] arguments)
        {
            this.key = key;
            this.table = table;
            Arguments = arguments;
            IsLocalized = true;
        }

        /// <summary>
        /// Initializes a new localized string.
        /// </summary>
        public LString(string key, string table)
        {
            this.key = key;
            this.table = table;
            IsLocalized = true;
        }

        /// <summary>
        /// Initializes a new implicitly localized string or nonlocalized string
        /// TODO: Add support for implicit table names [TableName/KeyName]
        /// </summary>
        public LString(string s)
        {
            if (s.EndsWith(IMPLICIT_LOCALIZATION_SUFFIX)) {
                if (s.StartsWith(IMPLICIT_LOCALIZATION_PREFIX)) {
                    //Implicitly localized string
                    key = s.Substring(IMPLICIT_LOCALIZATION_PREFIX.Length, s.Length - IMPLICIT_LOCALIZATION_PREFIX.Length - IMPLICIT_LOCALIZATION_SUFFIX.Length);
                    table = IMPLICIT_LOCALIZATION_DEFAULT_TABLE;
                    IsImplicit = true;
                } else if(s.StartsWith("\\" + IMPLICIT_LOCALIZATION_PREFIX)) {
                    //Escaped implicit localization
                    text = s.Substring(1);
                } else {
                    //Nonlocalized string
                    text = s;
                }
            } else {
                //Nonlocalized string
                text = s;
            }
        }

        public string Key
        {
            get {
                return key;
            }
            set {
                if(key == value) {
                    return;
                }
                key = value;
                if (string.IsNullOrEmpty(key)) {
                    Empty();
                } else {
                    text = null;
                }
            }
        }

        public string Table
        {
            get {
                return table;
            }
            set {
                if(table == value) {
                    return;
                }
                table = value;
                IsImplicit = false;
                if (string.IsNullOrEmpty(table)) {
                    Empty();
                } else {
                    text = null;
                }
            }
        }

        public string Text
        {
            get {
                if(text == null) {
                    if(table != null && key != null) {
                        text = Arguments == null ? Localization.All.Get(table, key) : string.Format(Localization.All.Get(table, key), Arguments);
                    } else {
                        text = string.Empty;
                    }
                }
                return text;
            }
        }

        private void Empty()
        {
            key = null;
            table = null;
            text = string.Empty;
            IsLocalized = false;
        }

        public static implicit operator string(LString lString)
        {
            return lString == null ? null : lString.Text;
        }

        public static implicit operator LString(string s)
        {
            return s == null ? null : new LString(s);
        }
    }
}
