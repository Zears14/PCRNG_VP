using Newtonsoft.Json;
using Spectre.Console;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PCRNG_VP.exTPM.Cryptography.KeyStoreProvider
{
    public class ExternalKeyStore : IKeyStoreProvider
    {
        private readonly string sksMountDir;
        private readonly string appTPMpath;
        private readonly string keyPath;
        private readonly string privateKeyPem;
        private readonly string KeyMetadataPath;
        private readonly KeyMetadata? KeyMetadataValue;

        public ExternalKeyStore(string baseDirectory)
        {
            // Initialize the rest during construction for better efficiency
            sksMountDir = Path.Combine(baseDirectory, "SKS");
            appTPMpath = Path.Combine(sksMountDir, Helpers.AppGuidRetrieval.GetAppGuid());
            keyPath = Path.Combine(appTPMpath, "keys");
            privateKeyPem = Path.Combine(keyPath, "encrypted_private_key.pem");
            KeyMetadataPath = Path.Combine(appTPMpath, "KeyMetadataPath.json");
            KeyMetadataValue = JsonConvert.DeserializeObject<KeyMetadata>(File.ReadAllText(KeyMetadataPath));
        }

        public void StoreKey(string KeyName, byte[] rawKeyBytes)
        {

            string appKeyPath = Path.Combine(keyPath, KeyName);

            if (File.Exists(appKeyPath))
            {
                return;
            }
            if (KeyMetadataValue == null)
            {
                throw new CryptographicException("Cannot get the key metadata");
            }
            if (KeyMetadataValue.KeySize == null)
            {
                throw new CryptographicException("Cannot retrieve key size from key metadata");
            }
            string AppKeypairPemValue = File.ReadAllText(privateKeyPem);
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = Convert.ToInt32(KeyMetadataValue.KeySize);

                string password = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter your key [green]password[/][bold italic dim] (your password is not shown)[/]:")
                        .PromptStyle("red")
                        .Secret(null));

                byte[] passwordByte = Encoding.UTF8.GetBytes(password);
                rsa.ImportFromEncryptedPem(AppKeypairPemValue, passwordByte);
                byte[] keyEncrypted = rsa.Encrypt(rawKeyBytes, RSAEncryptionPadding.OaepSHA256);
                using (FileStream fs = File.Create(appKeyPath))
                {
                    fs.Close();
                }
                File.WriteAllBytes(appKeyPath, keyEncrypted);
            }
        }

        public byte[] RetrieveKey(string KeyName)
        {
            string appKeyPath = Path.Combine(keyPath, KeyName);

            byte[] keyBytes = File.ReadAllBytes(appKeyPath);

            if (!File.Exists(appKeyPath))
            {
                throw new FileNotFoundException(KeyName + " does not exist on " + keyPath);
            }
            if (KeyMetadataValue == null)
            {
                throw new CryptographicException("Cannot get the key metadata");
            }
            if (KeyMetadataValue.KeySize == null)
            {
                throw new CryptographicException("Cannot retrieve key size from key metadata");
            }
            string AppKeypairPemValue = File.ReadAllText(privateKeyPem);
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = (int)KeyMetadataValue.KeySize;

                string password = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter your key [green]password[/][bold italic dim] (your password is not shown)[/]:")
                        .PromptStyle("red")
                        .Secret(null));

                byte[] passwordByte = Encoding.UTF8.GetBytes(password);
                rsa.ImportFromEncryptedPem(AppKeypairPemValue, passwordByte);
                byte[] keyDecrypted = rsa.Decrypt(keyBytes, RSAEncryptionPadding.OaepSHA256);
                return keyDecrypted;
            }
        }
    }
}
