using PCRNG_VP.DeveloperMode;
using System.Collections.Generic;
using System;
namespace PCRNG_VP.DeveloperMode.Commands
{
    public class GetLogsOnMemoryCommand : ICommand
    {
        public string Description => "Get the logs stored on the Logs variable in memory.";
        public string Execute(string[] args)
        {
            List<string> Logs = PCRNG_VP.ProgramLogger.Logs;
            return string.Join(Environment.NewLine, Logs);
        }
    }
}
