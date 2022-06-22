namespace Game.Utils.Config
{
    public class Config
    {
        public LogLevel LogLevel;
        public LogLevel LogConsoleLevel;
        public bool LogPrefix;
        public bool LogMethod;
        public string SaveFolder;
        public bool ConsoleEnabled;

        public static Config Default
        {
            get {
                return new Config() {
                    LogLevel = LogLevel.Debug,
                    LogConsoleLevel = LogLevel.Error,
                    LogPrefix = true,
                    LogMethod = true,
                    ConsoleEnabled = true
                };
            }
        }
    }
}