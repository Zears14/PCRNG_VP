using Spectre.Console;
using System;
using Console = Colorful.Console;

namespace PCRNG_VP.exTPM
{
    public static class TPMLogger
    {
        public enum LogLevel
        {
            INFO,
            WARN,
            ERROR
        }

        public static void Log(string message, LogLevel severity = LogLevel.INFO, string extra = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null or empty.");
            }

            string logMessage = $"{DateTime.Now:s} [TPM][{severity}{extra}]: {message}";

            Console.WriteLine(logMessage, GetConsoleColor(severity));
            Console.ResetColor();

            ProgramLogger.Logs.Add(logMessage);
        }

        public static void LogSilent(string message, LogLevel severity = LogLevel.INFO, string extra = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null or empty.");
            }

            string logMessage = $"{DateTime.Now:s} [TPM][{severity}{extra}]: {message}";

            ProgramLogger.Logs.Add(logMessage);
        }

        public static void HandleError(Exception ex)
        {
            LogSilent(ex.Message, LogLevel.ERROR, ": " + ex.GetType().ToString());
            LogSilent(ex.StackTrace ?? "Unable to get stack trace!", LogLevel.ERROR, ": STACK TRACE (" + ex.GetType().ToString() + ")");
            LogSilent("END STACK TRACE", LogLevel.ERROR, ": STACK TRACE (" + ex.GetType().ToString() + ")");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
        }

        private static System.Drawing.Color GetConsoleColor(LogLevel severity)
        {
            return severity switch
            {
                LogLevel.INFO => System.Drawing.Color.DarkCyan,
                LogLevel.WARN => System.Drawing.Color.Yellow,
                LogLevel.ERROR => System.Drawing.Color.Red,
                _ => System.Drawing.Color.White
            };

        }
    }
}
