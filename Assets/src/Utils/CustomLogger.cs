using Game.UI;
using Game.Utils.Config;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Game.Utils
{
    public class CustomLogger
    {
        public static LogLevel MinLevel { get; set; } = LogLevel.Debug;
        public static LogLevel MinConsoleLevel { get; set; } = LogLevel.Error;
        public static bool LogPrefix { get; set; } = true;
        public static bool LogMethod { get; set; } = true;

        public static void LoadSettings()
        {
            MinLevel = ConfigManager.Config.LogLevel;
            MinConsoleLevel = ConfigManager.Config.LogConsoleLevel;
            LogPrefix = ConfigManager.Config.LogPrefix;
            LogMethod = ConfigManager.Config.LogMethod;
            LogRaw("LOGGER - " + Localization.Log.Get("LoggerSettingsLoaded"));
        }

        /// <summary>
        /// Log a localized message
        /// </summary>
        public static void Log(LogLevel level, string key)
        {
            LogRaw(level, Localization.Log.Get(key));
        }

        /// <summary>
        /// Log a localized message
        /// </summary>
        public static void Log(LogLevel level, string key, params string[] arguments)
        {
            LogRaw(level, string.Format(Localization.Log.Get(key), arguments));
        }

        /// <summary>
        /// Log a raw string
        /// </summary>
        public static void LogRaw(LogLevel level, string format, params string[] arguments)
        {
            LogRaw(level, string.Format(format, arguments));
        }

        /// <summary>
        /// Log a raw string
        /// </summary>
        public static void LogRaw(LogLevel level, string message)
        {
            if((int)level < (int)MinLevel) {
                //Level too low
                return;
            }
            StringBuilder messageBuilder = new StringBuilder();
            if (LogPrefix) {
                messageBuilder.Append(level.ToString().ToUpper());
                if (LogMethod) {
                    messageBuilder.Append(" - ");
                } else {
                    messageBuilder.Append(": ");
                }
            }
            if (LogMethod) {
                StackTrace trace = new StackTrace();
                StackFrame frame = null;
                for (int i = 1; i < trace.FrameCount && frame == null; i++) {
                    if(trace.GetFrame(i).GetMethod().ReflectedType.Name != "CustomLogger") {
                        frame = trace.GetFrame(i);
                    }
                }
                messageBuilder.Append(frame != null ? frame.GetMethod().ReflectedType.Name : "UnknownClass");
                messageBuilder.Append(" -> ");
                messageBuilder.Append(frame != null ? ParseMethodName(frame.GetMethod()) : "UnknownMethod");
                messageBuilder.Append(": ");
            }

            messageBuilder.Append(message);

            WriteLog(messageBuilder.ToString());
            if((int)level >= (int)MinConsoleLevel && ConsoleManager.Instance != null) {
                ConsoleManager.Instance.RunCommand(string.Format("echo {0}", messageBuilder.ToString()), true);
            }
        }

        /// <summary>
        /// Log a raw string
        /// </summary>
        public static void LogRaw(string message)
        {
            WriteLog(message);
        }

        /// <summary>
        /// Log a localized debug message
        /// </summary>
        public static void Debug(string key)
        {
            Log(LogLevel.Debug, key);
        }

        /// <summary>
        /// Log a localized debug message
        /// </summary>
        public static void Debug(string key, params string[] arguments)
        {
            Log(LogLevel.Debug, key, arguments);
        }

        /// <summary>
        /// Log a debug message
        /// </summary>
        public static void DebugRaw(string message)
        {
            LogRaw(LogLevel.Debug, message);
        }

        /// <summary>
        /// Log a debug message
        /// </summary>
        public static void DebugRaw(string format, params string[] arguments)
        {
            LogRaw(LogLevel.Debug, format, arguments);
        }

        /// <summary>
        /// Log a localized warning message
        /// </summary>
        public static void Warning(string key)
        {
            Log(LogLevel.Warning, key);
        }

        /// <summary>
        /// Log a localized warning message
        /// </summary>
        public static void Warning(string key, params string[] arguments)
        {
            Log(LogLevel.Warning, key, arguments);
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void WarningRaw(string message)
        {
            LogRaw(LogLevel.Warning, message);
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void WarningRaw(string format, params string[] arguments)
        {
            LogRaw(LogLevel.Warning, format, arguments);
        }

        /// <summary>
        /// Log a localized error message
        /// </summary>
        public static void Error(string key)
        {
            Log(LogLevel.Error, key);
        }

        /// <summary>
        /// Log a localized error message
        /// </summary>
        public static void Error(string key, params string[] arguments)
        {
            Log(LogLevel.Error, key, arguments);
        }

        /// <summary>
        /// Log a error message
        /// </summary>
        public static void ErrorRaw(string message)
        {
            LogRaw(LogLevel.Error, message);
        }

        /// <summary>
        /// Log a error message
        /// </summary>
        public static void ErrorRaw(string format, params string[] arguments)
        {
            LogRaw(LogLevel.Error, format, arguments);
        }

        /// <summary>
        /// Replaces constructor abreviation
        /// </summary>
        /// <param name="methodBase"></param>
        /// <returns></returns>
        private static string ParseMethodName(MethodBase methodBase)
        {
            string name = methodBase.Name;
            if (name == ".ctor") {
                name = "Constructor";
            }
            return name;
        }

        private static void WriteLog(string message)
        {
            //TODO: Add text file log as an option? (path in config file?)
            UnityEngine.Debug.Log(message);
            /*if (ConsoleManager.Instance != null) {
                ConsoleManager.Instance.Run_Command("echo " + message);
            }*/
        }
    }
}
