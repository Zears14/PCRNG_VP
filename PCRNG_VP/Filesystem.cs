using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace PCRNG_VP
{
    public static class Filesystem
    {
        public static string FindVolumeByLabel(string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                throw new ArgumentNullException(nameof(label), $"'{nameof(label)}' cannot be null or empty.");
            }

            using Process process = new Process
            {
                StartInfo =
                {
                    FileName = "diskpart",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.StandardInput.WriteLine("list volume");
            process.StandardInput.WriteLine("exit");

            string output = process.StandardOutput.ReadToEnd();
            string[] lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            // Regular expression pattern to match the volume number and label
            string pattern = @"[*]? ?Volume (\d+)     ([A-Z])?   ( )?" + label;

            // Loop through each line and check for the label
            foreach (string? line in lines)
            {
                Match? match = Regex.Match(line ?? "", pattern); // Handle possible null line
                if (match.Success)
                {
                    string? volumeNumber = match.Groups[1].Value;

                    return "Volume " + volumeNumber; // Exit the method once the first match is found
                }
            }

            throw new FileNotFoundException($"Volume with label '{label}' is not found!");
        }


        public static void MountOrUnmountVolumeOnDriveLetter(string driveLetter, string volume, bool unmount = false)
        {
            if (string.IsNullOrEmpty(driveLetter))
            {
                throw new ArgumentNullException(nameof(driveLetter), $"'{nameof(driveLetter)}' cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(volume))
            {
                throw new ArgumentNullException(nameof(volume), $"'{nameof(volume)}' cannot be null or empty.");
            }

            using Process process = new Process
            {
                StartInfo =
                {
                    FileName = "diskpart",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.StandardInput.WriteLine("lis vol");
            process.StandardInput.WriteLine($"sel {volume}");

            process.StandardInput.WriteLine(unmount ? $"remove letter={driveLetter}" : $"assign letter={driveLetter}");

            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
        }
        public static void MountOrUnmountVolumeOnFolder(string path, string volume, bool unmount = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path), $"'{nameof(path)}' cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(volume))
            {
                throw new ArgumentNullException(nameof(volume), $"'{nameof(volume)}' cannot be null or empty.");
            }

            using Process process = new Process
            {
                StartInfo =
                {
                    FileName = "diskpart",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.StandardInput.WriteLine("lis vol");
            process.StandardInput.WriteLine($"sel {volume}");

            process.StandardInput.WriteLine(unmount ? $"remove mount={path}" : $"assign mount={path}");

            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
        }
    }
}
