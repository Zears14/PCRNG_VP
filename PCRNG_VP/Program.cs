﻿using System;
using Console = Colorful.Console;
using Spectre.Console;
using PCRNG_VP.exTPM;
using static PCRNG_VP.ProgramLogger;

namespace PCRNG_VP
{
    internal static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="System.NotImplementedException">Assembly GUID is not found</exception>
        private static void Main(string[] args)
        {
            try
            {
                ProgramInit.DoInit();
                Console.Clear();
                string userchoices = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[bold]Welcome to[/][bold cyan1] Picrypt Next[/][bold] Vault App (PCNRNG_VP)![/]")
                            .PageSize(3)
                            .AddChoices("Start", "Exit"));

                if (userchoices != null && userchoices == "Start")
                {
                    Program.Start();
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
        /// Starts the main program.
        /// </summary>
        static void Start()
        {
            Log("Setting up exTPM...", extra: ": TPMSETUP");
            exTPM.Basic.SetupConnection();

            TPMInitAndChecks.DoInitAndChecks();
            if (AnsiConsole.Confirm("[bold maroon] Unmount them?[/]"))
            {
                exTPM.Basic.CloseConnection();
            }
        }
    }
}

