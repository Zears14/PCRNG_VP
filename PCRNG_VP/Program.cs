using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Console = Colorful.Console;
using Spectre.Console;
using System.Text.Json.Nodes;


namespace PCRNG_VP
{
    internal static class Program
    {
        private static readonly string LogFilePath = $"log_{DateTime.Now:yyyy-MM-dd-HH_mm_ss_fffffff}.log";
#pragma warning disable S2223 // Non-constant static fields should not be visible
        public static List<string> Logs = new List<string>();
#pragma warning restore S2223 // Non-constant static fields should not be visible
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="System.NotImplementedException">Assembly GUID is not found</exception>
        private static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
                LogSilent("Setting up exit trap success!", extra: ": PREINIT");
                if (!File.Exists(LogFilePath))
                {
                    using (FileStream fs = File.Create(LogFilePath))
                    {
                        LogSilent("Creating log success!", extra: ": PREINIT");
                        fs.Close();
                    }
                }
                Log("Initliazing", extra: ": INIT");

                Assembly assembly = Assembly.GetExecutingAssembly();
                GuidAttribute? guidAttribute = assembly.GetCustomAttribute<GuidAttribute>();
                string guid = guidAttribute?.Value ?? throw new NotImplementedException("Assembly GUID is not found");
                string CurrentDir = Directory.GetCurrentDirectory();
                string MountDir = Path.Combine(CurrentDir, "FS", guid);
                Directory.CreateDirectory(MountDir);
#if DEBUG
                Console.WriteLine(guid);
                Console.WriteLine(CurrentDir);
                Console.WriteLine(MountDir);
                Console.ReadKey();
                Log("DONE", extra: ": INIT");
#endif
                Log("DONE", extra: ": INIT");
                Console.Clear();
                string userchoices = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[bold]Welcome to[/][bold cyan1] Picrypt Next[/][bold] Vault App (PCNRNG_VP)![/]")
                            .PageSize(3)
                            .AddChoices("Start", "Exit"));

                if (userchoices != null && userchoices == "Start")
                {
                    Start();
                }

            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
            finally
            {
                ConsoleExtend.AnyKeyToExit();
            }
        }

        /// <summary>
        /// Process exit handler and process resource cleanup.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException">Assembly GUID is not found</exception>
        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            LogSilent("Prepare Exit", extra: ": PRPEXT");
            Assembly assembly = Assembly.GetExecutingAssembly();
            GuidAttribute? guidAttribute = assembly.GetCustomAttribute<GuidAttribute>();
            string guid = guidAttribute?.Value ?? throw new NotImplementedException("Assembly GUID is not found");
            string CurrentDir = Directory.GetCurrentDirectory();
            LogSilent("FINISHED", extra: ": PRPEXT");
            LogSilent("Unmounting volume", extra: ": CLNUP");
            using Process process = new Process
            {
                StartInfo =
                {
                    FileName = "mountvol",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = CurrentDir+@"\FS"+guid+" /D"
                }
            };

            process.Start();
            process.WaitForExit();

            LogSilent("Deleting mountpoints", extra: ": CLNUP");
            Directory.Delete(CurrentDir + @"\FS", true);
            LogSilent("Saving logs", extra: ": CLNUP");
            foreach (string Log in Logs)
            {
                WriteToFile(Log);
            }
            Logs.Clear();
            WriteToFile($"{DateTime.Now:s} [PROGRAM][INFO: CLNUP]: Clean Up Finished, Exiting...");
        }

        /// <summary>
        /// Starts the main program.
        /// </summary>
        static void Start()
        {


            Log("Setting up exTPM...", extra: ": TPMSETUP");
            exTPM.Basic.SetupConnection();

            Assembly assembly = Assembly.GetExecutingAssembly();
            GuidAttribute? guidAttribute = assembly.GetCustomAttribute<GuidAttribute>();
            string guid = guidAttribute?.Value ?? throw new NotImplementedException("Assembly GUID is not found");
            string CurrentDir = Directory.GetCurrentDirectory();

            //Directory Variable
            string MountDir = Path.Combine(CurrentDir, "FS", guid);
            string sksMountDir = Path.Combine(MountDir, "SKS");
            string appTPMpath = Path.Combine(sksMountDir, guid);
            string keyPath = Path.Combine(appTPMpath, "keys");
            //File Path Variable
            string key_metadata = Path.Combine(appTPMpath, "key_metadata.json");
            string app_metadata = Path.Combine(appTPMpath, "app_metadata.json");
            string privateKeyPem = Path.Combine(keyPath, "encrypted_private_key.pem");


            if (!Directory.Exists(appTPMpath))
            {
                Log("App TPM folder not found, creating...", extra: ": TPMSETUP", severity: LogLevel.WARN);
                Directory.CreateDirectory(appTPMpath);
            }
            if (!Directory.Exists(keyPath))
            {
                Log("App TPM Keys folder not found, creating...", extra: ": TPMSETUP", severity: LogLevel.WARN);
                Directory.CreateDirectory(keyPath);
            }
            if (!File.Exists(key_metadata))
            {
                Log("App TPM keys_metadata.json not found, creating...", extra: ": TPMSETUP", severity: LogLevel.WARN);
                //TODO: Create and Fill the key_metadata.json
                using (FileStream fs = File.Create(key_metadata))
                {
                    fs.Close();
                }
            }
            if (!File.Exists(app_metadata))
            {
                //TODO: Create and Fill the app_metadata.json
                Log("App TPM app_metadata.json not found, creating...", extra: ": TPMSETUP", severity: LogLevel.WARN);

                using (FileStream fs = File.Create(app_metadata))
                {
                    fs.Close();
                }
            }
            if (!File.Exists(privateKeyPem))
            {
                //TODO:Do keygen
                Log("App TPM Private Keys not found (BAD), skipping...", extra: ": TPMSETUP", severity: LogLevel.WARN);

            }

            if (AnsiConsole.Confirm("[bold maroon] Unmount them?[/]"))
            {
                exTPM.Basic.CloseConnection();
            }
        }




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

            string logMessage = $"{DateTime.Now:s} [PROGRAM][{severity}{extra}]: {message}";

            Console.WriteLine(logMessage, GetConsoleColor(severity));
            Console.ResetColor();

            Logs.Add(logMessage);
        }

        public static void LogSilent(string message, LogLevel severity = LogLevel.INFO, string extra = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message), "Message cannot be null or empty.");
            }

            string logMessage = $"{DateTime.Now:s} [PROGRAM][{severity}{extra}]: {message}";
            Program.Logs.Add(logMessage);
        }

        public static void HandleError(Exception ex)
        {
            LogSilent(ex.Message, LogLevel.ERROR, ": " + ex.GetType().ToString());
            LogSilent(ex.StackTrace ?? "Unable to get stack trace!", LogLevel.ERROR, ": STACK TRACE (" + ex.GetType().ToString() + ")");
            LogSilent("END STACK TRACE", LogLevel.ERROR, ": STACK TRACE (" + ex.GetType().ToString() + ")");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths);
        }

        private static void WriteToFile(string logMessage)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(LogFilePath))
                {
                    sw.WriteLine(logMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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

