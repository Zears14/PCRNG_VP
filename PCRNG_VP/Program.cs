using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Console = Colorful.Console;


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
				AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
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
#endif
				Console.Clear();
				Start();
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
			Assembly assembly = Assembly.GetExecutingAssembly();
			GuidAttribute? guidAttribute = assembly.GetCustomAttribute<GuidAttribute>();
			string guid = guidAttribute?.Value ?? throw new NotImplementedException("Assembly GUID is not found");
            string CurrentDir = Directory.GetCurrentDirectory();
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

			Directory.Delete(CurrentDir+@"\FS", true);
		}

        /// <summary>
        /// Starts the program.
        /// </summary>
        static void Start()
		{
			Console.WriteLine("Start? (y/n):", System.Drawing.Color.White);
			string startInput = Console.ReadLine()?.ToLowerInvariant() ?? "n";
			if (startInput != "y")
			{
				Console.WriteLine("Alright, exiting...", System.Drawing.Color.Green);
				return;
			}
			
			exTPM.Basic.SetupConnection();
			Console.WriteLine("Unmount them? (y/n):", System.Drawing.Color.White);
			string userInput = Console.ReadLine()?.ToLowerInvariant() ?? "n";
			if (userInput == "y")
			{
				exTPM.Basic.CloseConnection();
			}
		}



        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="extra">Extra message tobe displayed.</param>
        /// <exception cref="System.ArgumentNullException">message -  cannot be null or empty.</exception>
        static void Log(string message, int severity = 0, string extra = "")
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentNullException(nameof(message), $"'{nameof(message)}' cannot be null or empty.");
			}

			if (severity == 0)
			{
				Console.Write($"[PROGRAM][INFO{extra}] ", System.Drawing.Color.DarkCyan);
				Console.Write(message + "\n");
			}
			else if (severity == 1)
			{
				Console.Write($"[PROGRAM][WARN{extra}] ", System.Drawing.Color.Yellow);
				Console.Write(message + "\n");
			}
			else if (severity == 2)
			{
				Console.Write($"[PROGRAM][ERROR{extra}] ", System.Drawing.Color.Red);
				Console.Write(message + "\n");
			}
		}
        /// <summary>
        /// Handles the error.
        /// </summary>
        /// <param name="ex">The exception.</param>
        static void HandleError(Exception ex)
		{
			Log(ex.Message, 2, ": " + ex.GetType().ToString());
			if (ex.StackTrace != null) { Log("\n" + ex.StackTrace.ToString(), extra: ": " + "STACK TRACE (" + ex.GetType().ToString() + ")"); }
			else { Log("Unable to get stack trace!", 1); }

		}
	}
}
