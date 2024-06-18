using System.Collections.Generic;
using System;

namespace PCRNG_VP.DeveloperMode.Commands
{
    public class HelpCommand : ICommand
    {
        public string Description => "Displays a list of available commands.";

        public string Execute(string[] args)
        {
            List<string> lines = new List<string> { "Available commands:" };

            foreach (var command in DeveloperShell.Commands)
            {
                lines.Add($"{command.Key} - {command.Value.Description}");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
