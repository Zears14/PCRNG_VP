using Spectre.Console;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using static PCRNG_VP.exTPM.TPMLogger;


namespace PCRNG_VP.exTPM
{
    public static class TPMInitAndChecks
    {
        /// <summary>
        /// Does the initialize and checks.
        /// </summary>
        /// <exception cref="NotImplementedException">Assembly GUID is not found</exception>
        public static void DoInitAndChecks()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            GuidAttribute? guidAttribute = assembly.GetCustomAttribute<GuidAttribute>();
            string guid = guidAttribute?.Value ?? throw new NotImplementedException("Assembly GUID is not found");
            string CurrentDir = Directory.GetCurrentDirectory();

            //Directory Variable
            string MountDir = Path.Combine(CurrentDir, "FS", guid);
            string sksMountDir = Path.Combine(MountDir, "SKS");
            string appTPMpath = Path.Combine(sksMountDir, guid);
            string keyPath = Path.Combine(appTPMpath, "keys");
            //File Path Variable
            string key_metadata = Path.Combine(appTPMpath, "key_metadata.json");
            string app_metadata = Path.Combine(appTPMpath, "app_metadata.json");
            string privateKeyPem = Path.Combine(keyPath, "encrypted_private_key.pem");


            if (!Directory.Exists(appTPMpath))
            {
                Log("App TPM folder not found, creating...", extra: ": TPMSETUP", severity: LogLevel.WARN);
                Directory.CreateDirectory(appTPMpath);
            }
            if (!Directory.Exists(keyPath))
            {
                Log("App TPM Keys folder not found, creating...", extra: ": TPMSETUP", severity: LogLevel.WARN);
                Directory.CreateDirectory(keyPath);
            }
            if (!File.Exists(key_metadata))
            {
                Log("App TPM keys_metadata.json not found, creating...", extra: ": TPMSETUP", severity: LogLevel.WARN);
                // NOTE: the key_metadata will be filled when key generation occur.
                using (FileStream fs = File.Create(key_metadata))
                {
                    fs.Close();
                }
            }
            if (!File.Exists(app_metadata))
            {
                //TODO: Create and Fill the app_metadata.json

                Log("App TPM app_metadata.json not found, creating...", extra: ": TPMSETUP", severity: LogLevel.WARN);

                using (FileStream fs = File.Create(app_metadata))
                {
                    fs.Close();
                }
            }
            if (!File.Exists(privateKeyPem))
            {
                Log("App TPM Private Keys not found, creating...", extra: ": TPMSETUP", severity: LogLevel.WARN);
                Crypto.TPMCreateKeysForApp(privateKeyPem);
            }
        }
    }
}
