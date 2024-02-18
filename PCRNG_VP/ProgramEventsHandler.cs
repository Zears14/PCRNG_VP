using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using static PCRNG_VP.ProgramLogger;

namespace PCRNG_VP
{
    public static class ProgramEventsHandler
    {
        /// <summary>
        /// Process exit handler and process resource cleanup.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException">Assembly GUID is not found</exception>
        public static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
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
    }
}
