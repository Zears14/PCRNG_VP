using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using static PCRNG_VP.ProgramLogger;

namespace PCRNG_VP
{
    public static class ProgramInit
    {
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
            Console.WriteLine(guid);
            Console.WriteLine(CurrentDir);
            Console.WriteLine(MountDir);
            Console.ReadKey();
            Log("(DEBUG) Continue-ing program...", extra: ": INIT");
#endif
            Log("DONE", extra: ": INIT");
            
        }
    }
}
