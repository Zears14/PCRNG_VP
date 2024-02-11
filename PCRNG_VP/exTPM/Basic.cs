using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace PCRNG_VP.exTPM
{
    public static class Basic
    {
        /// <summary>
        /// Setups exTPM connection.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">Assembly GUID is not found</exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// SKS partition is not found!
        /// or
        /// CNTRL partition is not found!
        /// </exception>
        public static bool SetupConnection()
        {
            Console.Clear();
            Assembly assembly = Assembly.GetExecutingAssembly();
            GuidAttribute? guidAttribute = assembly.GetCustomAttribute<GuidAttribute>();
            string guid = guidAttribute?.Value ?? throw new NotImplementedException("Assembly GUID is not found");
            string CurrentDir = Directory.GetCurrentDirectory();
            string MountDir = Path.Combine(CurrentDir, "FS", guid);
            string SKSMountDir = Path.Combine(MountDir, "SKS");
            string CNTRLMountDir = Path.Combine(MountDir, "CNTRL");
            Directory.CreateDirectory(SKSMountDir);
            Directory.CreateDirectory(CNTRLMountDir);



            Task<string?> sksTask = Task.Run(() =>
            {
                try
                {
                    return Filesystem.FindVolumeByLabel("exTPM-SKS");
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                    return null; // Or throw the exception again if appropriate
                }
            });

            Log("Finding partition: exTPM-SKS. On all drive...", extra: ": VOLFIND");
            Task.WaitAll(sksTask);
            string sksPartitionPath = sksTask.Result ?? throw new FileNotFoundException($"SKS partition is not found!");

            Task<string?> cntrlTask = Task.Run(() =>
            {
                try
                {
                    return Filesystem.FindVolumeByLabel("exTPM-CNTRL");
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                    return null; // Or throw the exception again if appropriate
                }
            });

            Log("Finding partition: exTPM-CNTRL. On all drive...", extra: ": VOLFIND");
            Task.WaitAll(cntrlTask);
            string cntrlPartitionPath = cntrlTask.Result ?? throw new FileNotFoundException($"CNTRL partition is not found!");

            Log("Finished finding: exTPM-SKS, exTPM-CNTRL", extra: ": VOLFIND");

            Log("Mounting exTPM-SKS partition...", extra: ": MNTEXTPM");
            Task sksMountTask = Task.Run(() =>
            {
                try
                {
                    Filesystem.MountOrUnmountVolumeOnFolder(SKSMountDir, sksPartitionPath);
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
            });

            Log("Mounting exTPM-CNTRL partition...", extra: ": MNTEXTPM");
            Task.WaitAll(sksMountTask);
            Task cntrlMountTask = Task.Run(() =>
            {
                try
                {
                    Filesystem.MountOrUnmountVolumeOnFolder(CNTRLMountDir, cntrlPartitionPath);
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
            });

            Task.WaitAll(cntrlMountTask);
            Log("Finished", extra: ": MNTEXTPM");
            return true;
        }
        /// <summary>
        /// Closes the exTPM connection.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException">Assembly GUID is not found</exception>
        /// <exception cref="System.IO.FileNotFoundException">
        /// SKS partition is not found!
        /// or
        /// CNTRL partition is not found!
        /// </exception>
        public static bool CloseConnection()
        {
            Console.Clear();
            Assembly assembly = Assembly.GetExecutingAssembly();
            GuidAttribute? guidAttribute = assembly.GetCustomAttribute<GuidAttribute>();
            string guid = guidAttribute?.Value ?? throw new NotImplementedException("Assembly GUID is not found");
            string CurrentDir = Directory.GetCurrentDirectory();
            string MountDir = Path.Combine(CurrentDir, "FS", guid);
            string SKSMountDir = Path.Combine(MountDir, "SKS");
            string CNTRLMountDir = Path.Combine(MountDir, "CNTRL");
            Directory.CreateDirectory(SKSMountDir);
            Directory.CreateDirectory(CNTRLMountDir);



            Task<string?> sksTask = Task.Run(() =>
            {
                try
                {
                    return Filesystem.FindVolumeByLabel("exTPM-SKS");
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                    return null; // Or throw the exception again if appropriate
                }
            });

            Log("Finding partition: exTPM-SKS. On all drive...", extra: ": VOLFIND");
            Task.WaitAll(sksTask);
            string sksPartitionPath = sksTask.Result ?? throw new FileNotFoundException($"SKS partition is not found!");

            Task<string?> cntrlTask = Task.Run(() =>
            {
                try
                {
                    return Filesystem.FindVolumeByLabel("exTPM-CNTRL");
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                    return null; // Or throw the exception again if appropriate
                }
            });

            Log("Finding partition: exTPM-CNTRL. On all drive...", extra: ": VOLFIND");
            Task.WaitAll(cntrlTask);
            string cntrlPartitionPath = cntrlTask.Result ?? throw new FileNotFoundException($"CNTRL partition is not found!");

            Log("Finished finding: exTPM-SKS, exTPM-CNTRL", extra: ": VOLFIND");
            Log("Unmounting exTPM-SKS partition...", extra: ": UMNTEXTPM");
            Task sksunMountTask = Task.Run(() =>
            {
                try
                {
                    Filesystem.MountOrUnmountVolumeOnFolder(SKSMountDir, sksPartitionPath, true);
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
            });
            Task.WaitAll(sksunMountTask);
            Log("Unmounting exTPM-CNTRL partition...", extra: ": UMNTEXTPM");
            Task cntrlunMountTask = Task.Run(() =>
            {
                try
                {
                    Filesystem.MountOrUnmountVolumeOnFolder(CNTRLMountDir, cntrlPartitionPath, true);
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
            });
            Task.WaitAll(cntrlunMountTask);


            Log("Finished, You may unplug the USB stick", extra: ": UMNTEXTPM");
            return true;
        }
        static void Log(string message, int severity = 0, string extra = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException(nameof(message), $"'{nameof(message)}' cannot be null or empty.");
            }

            if (severity == 0)
            {
                Console.Write($"[TPM][INFO{extra}] ", System.Drawing.Color.DarkCyan);
                Console.Write(message + "\n");
            }
            else if (severity == 1)
            {
                Console.Write($"[TPM][WARN{extra}] ", System.Drawing.Color.Yellow);
                Console.Write(message + "\n");
            }
            else if (severity == 2)
            {
                Console.Write($"[TPM][ERROR{extra}] ", System.Drawing.Color.Red);
                Console.Write(message + "\n");
            }
        }
        static void HandleError(Exception ex)
        {
            Log(ex.Message, 2, ": " + ex.GetType().ToString());
            if (ex.StackTrace != null) { Log("\n" + ex.StackTrace.ToString(), extra: ": " + "STACK TRACE (" + ex.GetType().ToString() + ")"); }
            else { Log("Unable to get stack trace!", 1); }

        }
    }
}
