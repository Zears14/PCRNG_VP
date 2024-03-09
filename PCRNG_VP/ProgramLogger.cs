using System;
using Spectre.Console;
using System.IO;
using System.Collections.Generic;
using Console = Colorful.Console; 

namespace PCRNG_VP
{
    public static class ProgramLogger
    {
        public static readonly string LogFilePath = $"log_{DateTime.Now:yyyy-MM-dd-HH_mm_ss_fffffff}.log";

#pragma warning disable S2223 // Non-constant static fields should not be visible
        public static List<string> Logs = new List<string>();
#pragma warning restore S2223 // Non-constant static fields should not be visible

        public enum LogLevel
        {
            DEBUG,
            INFO,
            WARN,
            ERROR
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message of the logs.</param>
        /// <param name="severity">The severity of the logs (Default to INFO).</param>
        /// <param name="extra">Extra Logs message.</param>
        /// <exception cref="ArgumentNullException">message - Message cannot be null or empty.</exception>
        public static void Log(string message, LogLevel severity = LogLevel.INFO, string extra = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null or empty.");
            }

            string logMessage = $"{DateTime.Now:s} [PROGRAM][{severity}{extra}]: {message}";

            Console.WriteLine(logMessage, GetConsoleColor(severity));
            Console.ResetColor();

            ProgramLogger.Logs.Add(logMessage);
        }

        /// <summary>
        /// Logs silently the specified message.
        /// </summary>
        /// <param name="message">The message of the logs.</param>
        /// <param name="severity">The severity of the logs (Default to INFO).</param>
        /// <param name="extra">Extra Logs message.</param>
        /// <exception cref="ArgumentNullException">message - Message cannot be null or empty.</exception>
        public static void LogSilent(string message, LogLevel severity = LogLevel.INFO, string extra = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null or empty.");
            }

            string logMessage = $"{DateTime.Now:s} [PROGRAM][{severity}{extra}]: {message}";

            ProgramLogger.Logs.Add(logMessage);
        }

        /// <summary>
        /// Handles the error.
        /// </summary>
        /// <param name="ex">The error to be handled.</param>
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
                LogLevel.DEBUG => System.Drawing.Color.WhiteSmoke,
                LogLevel.INFO => System.Drawing.Color.DarkCyan,
                LogLevel.WARN => System.Drawing.Color.Yellow,
                LogLevel.ERROR => System.Drawing.Color.Red,
                _ => System.Drawing.Color.White
            };

        }

        public static void WriteToFile(string logMessage)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(LogFilePath))
                {
                    sw.WriteLine(logMessage);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
