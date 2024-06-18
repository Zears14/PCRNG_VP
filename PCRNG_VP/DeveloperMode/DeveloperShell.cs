using PCRNG_VP.DeveloperMode.Commands;
using System;
using System.Collections.Generic;
using Spectre.Console;

namespace PCRNG_VP.DeveloperMode
{
    public class DeveloperShell
    {
        public static readonly Dictionary<string, ICommand> Commands = new Dictionary<string, ICommand>();

        public DeveloperShell()
        {
            // Register commands
            RegisterCommand("get_logs_on_mem", new GetLogsOnMemoryCommand());
            RegisterCommand("help", new HelpCommand());
            RegisterCommand("mem_dmp", new MemoryDumpCommand());
            RegisterCommand("clear", new ClearCommand());
        }

        public void StartShell()
        {
            // Display header
            AnsiConsole.Write(
                new FigletText("Developer Mode")
                    .Centered()
                    .Color(Color.Yellow));

            // Display disclaimer
            AnsiConsole.MarkupLine("[bold yellow]DISCLAIMER[/]");
            AnsiConsole.MarkupLine("[bold red]==============================[/]");
            AnsiConsole.MarkupLine("[bold]You are about to enter Developer Mode. This mode is intended for advanced users only. The developer of this application is [underline]not liable for any data loss, damage, or security issues[/] caused by the use of Developer Mode. By proceeding, you acknowledge that you understand the risks and take full responsibility for any actions taken in this mode. This includes, but is not limited to, the potential exposure of sensitive information such as decrypted private keys or other confidential data in memory. Mishandling such information, including improper handling of memory dumps, can lead to severe security vulnerabilities. If you are unsure about any operation, it is strongly recommended that you do not proceed.[/]");
            AnsiConsole.MarkupLine("[bold red]==============================[/]");

            // Confirmation prompt
            if (!AnsiConsole.Confirm("Do you wish to proceed to Developer Mode? [bold](y/n)[/]", false))
            {
                AnsiConsole.MarkupLine("[bold red]Exiting Developer Mode.[/]");
                return;
            }

            AnsiConsole.MarkupLine("[bold yellow]Developer Mode. Type 'exit' to quit.[/]");

            while (true)
            {
                AnsiConsole.Markup("[bold green]dev$[/] ");
                string? input = Console.ReadLine();

                if (input != null && input.Trim().ToLower() == "exit")
                    break;
                if (!string.IsNullOrEmpty(input))
                {
                    ExecuteCommand(input);
                }
            }
        }

        private static void RegisterCommand(string name, ICommand command)
        {
            Commands[name] = command;
        }

        private static void ExecuteCommand(string input)
        {
            string[] parts = input.Split(' ');
            string commandName = parts[0];
            string[] commandArgs = parts.Length > 1 ? parts[1..] : new string[0];

            if (Commands.TryGetValue(commandName, out ICommand? command))
            {
                if (command != null)
                {
                    string result = command.Execute(commandArgs);
                    AnsiConsole.MarkupLine("[bold blue]{0}[/]", Markup.Escape(result));
                }
            }
            else
            {
                AnsiConsole.MarkupLine($"[bold red]Command '{Markup.Escape(commandName)}' not found.[/]");
            }
        }
        
    }
}
