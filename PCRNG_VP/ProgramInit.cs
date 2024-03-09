using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using static PCRNG_VP.ProgramLogger;

namespace PCRNG_VP
{
    public static class ProgramInit
    {
        /// <summary>
        /// Does the initialization.
        /// </summary>
        /// <exception cref="System.NotImplementedException">Assembly GUID is not found</exception>
        public static void DoInit()
        {
            AppDomain.CurrentDomain.ProcessExit += ProgramEventsHandler.CurrentDomain_ProcessExit;
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
            Log($"Application GUID: {guid}", LogLevel.DEBUG, ": INIT-DEBUG-TIME");
            Log($"Application Current Directory: {CurrentDir}", LogLevel.DEBUG, ": INIT-DEBUG-TIME");
            Log($"Application Mount Directory: {MountDir}", LogLevel.DEBUG, ": INIT-DEBUG-TIME");
            Console.ReadKey();
            Log("INIT-DEBUG-TIME complete, Resuming program init...", extra: ": INIT-DEBUG-TIME", severity: LogLevel.DEBUG);
#endif
            Log("DONE", extra: ": INIT");
            
        }
    }
}
