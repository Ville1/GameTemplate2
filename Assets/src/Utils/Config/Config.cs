namespace Game.Utils.Config
{
    public class Config
    {
        public LogLevel LogLevel;
        public LogLevel LogConsoleLevel;
        public bool LogPrefix;
        public bool LogMethod;
        public string SaveFolder;
        public float SavingTargetFPS;
        public bool ConsoleEnabled;

        public static Config Default
        {
            get {
                return new Config() {
                    LogLevel = LogLevel.Debug,
                    LogConsoleLevel = LogLevel.Error,
                    LogPrefix = true,
                    LogMethod = true,
                    SaveFolder = "C:/",
                    SavingTargetFPS = 60.0f,
                    ConsoleEnabled = true
                };
            }
        }
    }
}