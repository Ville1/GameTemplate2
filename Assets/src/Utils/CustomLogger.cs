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
            Log("LOGGER - " + Localization.Log.Get("LoggerSettingsLoaded"));
        }

        public static void Log(LString message)
        {
            WriteLog(message);
        }

        public static void Log(LogLevel level, LString message)
        {
            PrintLog(level, message);
        }

        private static void PrintLog(LogLevel level, LString format, params object[] arguments)
        {
            if (format.IsImplicit) {
                //Implicitly localized string, change default table to Log
                format.Table = LTables.Log;
            }
            PrintLog(level, string.Format(format, arguments));
        }

        private static void PrintLog(LogLevel level, LString message)
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

            if (message.IsImplicit) {
                //Implicitly localized string, change default table to Log
                message.Table = LTables.Log;
            }

            messageBuilder.Append(message);

            WriteLog(messageBuilder.ToString());
            if((int)level >= (int)MinConsoleLevel && ConsoleManager.Instance != null) {
                ConsoleManager.Instance.RunCommand(string.Format("echo {0}", messageBuilder.ToString()), true);
            }
        }

        public static void Debug(LString message)
        {
            PrintLog(LogLevel.Debug, message);
        }

        public static void Debug(LString format, params object[] arguments)
        {
            PrintLog(LogLevel.Debug, format, arguments);
        }

        public static void Warning(LString message)
        {
            PrintLog(LogLevel.Warning, message);
        }

        public static void Warning(LString format, params object[] arguments)
        {
            PrintLog(LogLevel.Warning, format, arguments);
        }

        public static void Error(LString message)
        {
            PrintLog(LogLevel.Error, message);
        }

        public static void Error(LString format, params object[] arguments)
        {
            PrintLog(LogLevel.Error, format, arguments);
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
