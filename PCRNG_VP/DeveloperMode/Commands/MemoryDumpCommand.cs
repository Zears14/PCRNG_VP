using Microsoft.Diagnostics.NETCore.Client;
using System;
using System.IO;

namespace PCRNG_VP.DeveloperMode.Commands
{
    public class MemoryDumpCommand : ICommand
    {
        public string Description => "Takes a full memory dump of this application.";

        public string Execute(string[] args)
        {
            try
            {
                string dumpPath = args.Length > 0 ? args[0] : $"dump_{DateTime.Now:yyyyMMdd_HHmmss}.dmp";
                int processId = Environment.ProcessId;

                var client = new DiagnosticsClient(processId);
                client.WriteDump(DumpType.Full, dumpPath);

                return $"Memory dump created successfully at {dumpPath}";
            }
            catch (Exception ex)
            {
                return $"Failed to create memory dump: {ex.Message}";
            }
        }
    }
}
