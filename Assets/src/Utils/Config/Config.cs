namespace Game.Utils.Config
{
    public class Config
    {
        public LogLevel LogLevel;
        public bool LogPrefix;
        public bool LogMethod;
        public string SaveFolder;

        public static Config Default
        {
            get {
                return new Config() {
                    LogLevel = LogLevel.Debug,
                    LogPrefix = true,
                    LogMethod = true
                };
            }
        }
    }
}