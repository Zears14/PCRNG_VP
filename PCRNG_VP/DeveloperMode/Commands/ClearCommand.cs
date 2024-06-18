using System.Collections.Generic;
using System;
using Spectre.Console;

namespace PCRNG_VP.DeveloperMode.Commands
{
    public class ClearCommand : ICommand
    {
        public string Description => "Clears the console.";

        public string Execute(string[] args)
        {
            AnsiConsole.Clear();
            Console.Clear();
            return "";
        }
    }
}
