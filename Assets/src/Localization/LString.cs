//Note: keep this in root namespace for ease of use
namespace Game
{
    public class LString
    {
        public static readonly string IMPLICIT_LOCALIZATION_PREFIX = "{";
        public static readonly string IMPLICIT_LOCALIZATION_SUFFIX = "}";
        public static readonly string IMPLICIT_LOCALIZATION_TABLE_NAME_SEPARATOR = "/";
        public static readonly string IMPLICIT_LOCALIZATION_DEFAULT_TABLE = "Game";

        public object[] Arguments { get; private set; } = null;
        public bool IsLocalized { get; private set; } = false;
        public bool UsesImplicitDefaultTable { get; private set; } = false;
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
        /// TODO: Write down syntax explanation / examples
        /// </summary>
        public LString(string s)
        {
            int minLength = IMPLICIT_LOCALIZATION_PREFIX.Length + IMPLICIT_LOCALIZATION_SUFFIX.Length;
            if (s.EndsWith(IMPLICIT_LOCALIZATION_SUFFIX) && s.Length > minLength) {
                if (s.StartsWith(IMPLICIT_LOCALIZATION_PREFIX)) {
                    //Implicitly localized string
                    if (s.Contains(IMPLICIT_LOCALIZATION_TABLE_NAME_SEPARATOR) && s.Length > minLength + IMPLICIT_LOCALIZATION_TABLE_NAME_SEPARATOR.Length + 1) {
                        //Table name defined
                        int splitIndex = s.IndexOf(IMPLICIT_LOCALIZATION_TABLE_NAME_SEPARATOR);
                        table = s.Substring(IMPLICIT_LOCALIZATION_PREFIX.Length, splitIndex - 1);
                        key = s.Substring(splitIndex + 1, s.Length - splitIndex - IMPLICIT_LOCALIZATION_SUFFIX.Length - 1);
                    } else {
                        //Use default table
                        key = s.Substring(IMPLICIT_LOCALIZATION_PREFIX.Length, s.Length - IMPLICIT_LOCALIZATION_PREFIX.Length - IMPLICIT_LOCALIZATION_SUFFIX.Length);
                        table = IMPLICIT_LOCALIZATION_DEFAULT_TABLE;
                        UsesImplicitDefaultTable = true;
                    }
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
                UsesImplicitDefaultTable = false;
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

        public void ChangeImplicitDefaultTable(string table)
        {
            if (UsesImplicitDefaultTable) {
                Table = table;
            }
        }

        private void Empty()
        {
            key = null;
            table = null;
            text = string.Empty;
            IsLocalized = false;
        }

        public override string ToString()
        {
            return Text;
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
