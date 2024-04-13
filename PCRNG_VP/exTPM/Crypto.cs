using System;
using System.IO.Compression;
using System.IO;
using System.Formats.Tar;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using static PCRNG_VP.exTPM.Types;
using Spectre.Console;
using System.Reflection;
using System.Runtime.InteropServices;
using K4os.Compression.LZ4;
using System.Security.AccessControl;
namespace PCRNG_VP.exTPM
{
    public static class Crypto
    {
        private const int AesKeySize = 32; // AES-256 key size in bytes
        private const int AesNonceSize = 12; // AES-GCM nonce size in bytes
        private const int AesTagSize = 16; // AES-GCM tag size in bytes
        /// <summary>
        /// Generate random bytes.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static byte[] GenerateRandomBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        /// <summary>
        /// Generates the RSA keypair to encrypted pem.
        /// </summary>
        /// <param name="KeySize">Size of the key.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public static string GenerateRSAKeypairToEncryptedPEMEncoded(int KeySize, string password)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = KeySize;
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                PbeParameters pbeParameters = new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA512, 10000);

                string key = rsa.ExportEncryptedPkcs8PrivateKeyPem(passwordBytes, pbeParameters);
                return key;
            }
        }

        /// <summary>
        ///Create keys for application.
        /// </summary>
        /// <param name="KeyOutputPath">The key output path.</param>
        /// <exception cref="System.NotImplementedException">Assembly GUID is not found</exception>
        public static void TPMCreateKeysForApp(string KeyOutputPath)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            GuidAttribute? guidAttribute = assembly.GetCustomAttribute<GuidAttribute>();
            string guid = guidAttribute?.Value ?? throw new NotImplementedException("Assembly GUID is not found");
            string CurrentDir = Directory.GetCurrentDirectory();

            //Directory Variable
            string MountDir = Path.Combine(CurrentDir, "FS", guid);
            string sksMountDir = Path.Combine(MountDir, "SKS");
            string appTPMpath = Path.Combine(sksMountDir, guid);
            string key_metadata = Path.Combine(appTPMpath, "key_metadata.json");

            int keySize = AnsiConsole.Prompt(
            new SelectionPrompt<int>()
                .Title("Select RSA Key Size:")
                .PageSize(3)
                .AddChoices(2048, 4096, 8192));

            if (keySize == 2048 && !AnsiConsole.Confirm("2048 bit key are secure but [bold]its bare minimum[/], are you sure you want to use this size?"))
            {
                keySize = AnsiConsole.Prompt(
        new SelectionPrompt<int>()
            .Title("Select RSA Key Size:").PageSize(3).AddChoices(2048, 4096, 8192));
            }

            if (keySize == 8192 && !AnsiConsole.Confirm("8192 bit key are very secure but [bold] it may take some or even a long time to make[/], are you sure you want to use this size?"))
            {
                keySize = AnsiConsole.Prompt(
        new SelectionPrompt<int>()
            .Title("Select RSA Key Size:").PageSize(3).AddChoices(2048, 4096, 8192));
            }
            string password;
            string confirmPassword;

            while (true)
            {
                // Prompt the user to enter their password
                password = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter your key [green]password[/][bold italic dim] (your password is not shown)[/]:")
                        .PromptStyle("red")
                        .ValidationErrorMessage("[red]Password must be at least 6 characters long[/]")
                        .Secret(null)
                        .Validate(password =>
                        {
                            return password.Length >= 6 ? ValidationResult.Success() : ValidationResult.Error("Password must be at least 6 characters long");
                        }));

                // Prompt the user to repeat their password for confirmation
                confirmPassword = AnsiConsole.Prompt(
                    new TextPrompt<string>("Repeat your key [green]password[/][bold italic dim] (your password is not shown)[/]:")
                        .PromptStyle("red")
                        .Secret(null));

                // Check if the passwords match
                if (password == confirmPassword)
                {
                    break;
                }

                AnsiConsole.MarkupLine("[red]Passwords do not match. Please try again.[/]");
            }

            string key = exTPM.Crypto.GenerateRSAKeypairToEncryptedPEMEncoded(keySize, password);
            using (FileStream fs = File.Create(KeyOutputPath))
            {
                fs.Close();
            }
            using (StreamWriter sw = new StreamWriter(KeyOutputPath))
            {
                sw.WriteLine(key);
                sw.Close();
            }
            KeyMetadata keyMetadata = new KeyMetadata
            {
                KeyType = Types.KeyType.Asymmetric, // Example: Set the KeyType to Asymmetric
                KeyAlgorithm = Types.KeyAlgorithm.RSA, // Example: Set the KeyAlgorithm to RSA
                KeySize = keySize, // Convert keySize to string and set it as KeySize
                KeyFingerprint = "SHA256:" + ComputeHash(Encoding.UTF8.GetBytes(key), HashingAlgorithm.SHA256) // Calculate the fingerprint and set it
            };
            string KeyMetadataContents = JsonConvert.SerializeObject(keyMetadata, Formatting.Indented);
            File.WriteAllText(key_metadata, KeyMetadataContents);
        }
        public static void TPMStoreKeySecurely(byte[] keyBytes, string KeyName)
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
            string privateKeyPem = Path.Combine(keyPath, "encrypted_private_key.pem");
            string key_metadata = Path.Combine(appTPMpath, "key_metadata.json");
            string appKeyPath = Path.Combine(keyPath, KeyName);

            KeyMetadata? keyMetadata = JsonConvert.DeserializeObject<KeyMetadata>(File.ReadAllText(key_metadata));


            if (File.Exists(appKeyPath))
            {
                return;
            }
            if (keyMetadata == null)
            {
                throw new CryptographicException("Cannot get the key metadata");
            }
            if (keyMetadata.KeySize == null)
            {
                throw new CryptographicException("Cannot retrieve key size for key metadata");
            }
            string AppKeypairPemValue = File.ReadAllText(privateKeyPem);
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = (int)keyMetadata.KeySize;
                string password = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter your key [green]password[/][bold italic dim] (your password is not shown)[/]:")
                        .PromptStyle("red")
                        .Secret(null));
                byte[] passwordByte = Encoding.UTF8.GetBytes(password);
                rsa.ImportFromEncryptedPem(AppKeypairPemValue, passwordByte);
                byte[] keyEncrypted = rsa.Encrypt(keyBytes, RSAEncryptionPadding.OaepSHA256);
                using (FileStream fs = File.Create(appKeyPath))
                {
                    fs.Close();
                }
                File.WriteAllBytes(appKeyPath, keyEncrypted);
            }
        }
        public static byte[] TPMRetrieveKeySecurely(string KeyName)
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
            string privateKeyPem = Path.Combine(keyPath, "encrypted_private_key.pem");
            string appKeyPath = Path.Combine(keyPath, KeyName);
            string key_metadata = Path.Combine(appTPMpath, "key_metadata.json");

            byte[] keyBytes = File.ReadAllBytes(appKeyPath);

            if (!File.Exists(appKeyPath))
            {
                throw new FileNotFoundException(KeyName + " does not exist on "+keyPath);
            }
            KeyMetadata? keyMetadata = JsonConvert.DeserializeObject<KeyMetadata>(File.ReadAllText(key_metadata));

            if (keyMetadata == null)
            {
                throw new CryptographicException("Cannot get the key metadata");
            }
            if (keyMetadata.KeySize == null)
            {
                throw new CryptographicException("Cannot retrieve key size for key metadata");
            }
            string AppKeypairPemValue = File.ReadAllText(privateKeyPem);
            using (RSA rsa = RSA.Create())
            {
                rsa.KeySize = (int)keyMetadata.KeySize;
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

        /// <summary>
        /// Computes the hash of the data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="hashingAlgorithm">The hashing algorithm.</param>
        /// <returns></returns>
        public static string ComputeHash(byte[] data, HashingAlgorithm hashingAlgorithm)
        {
            using (var hasher = GetHashAlgorithm(hashingAlgorithm))
            {
                byte[] hashBytes = hasher.ComputeHash(data);
                return BitConverter.ToString(hashBytes).Replace("-", ""); // Convert hash bytes to hexadecimal string
            }
        }

        /// <summary>
        /// Gets the hash algorithm.
        /// </summary>
        /// <param name="hashAlgorithm">The hash algorithm.</param>
        /// <returns></returns>
        private static HashAlgorithm GetHashAlgorithm(HashingAlgorithm hashAlgorithm)
        {
            return hashAlgorithm switch
            {
                HashingAlgorithm.SHA256 => SHA256.Create(),
                HashingAlgorithm.SHA512 => SHA512.Create(),
                // Add other hash algorithms as needed
                _ => throw new NotSupportedException("Unsupported hash algorithm.")
            };
        }

        //Begin Main Encryption Logic
        public static void EncryptFile(string inputFile, string outputFile)
        {
            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException(inputFile + " does not exist.");
            }
            Assembly assembly = Assembly.GetExecutingAssembly();
            GuidAttribute? guidAttribute = assembly.GetCustomAttribute<GuidAttribute>();
            string guid = guidAttribute?.Value ?? throw new NotImplementedException("Assembly GUID is not found");
            string CurrentDir = Directory.GetCurrentDirectory();

            //Directory Variable
            string MountDir = Path.Combine(CurrentDir, "FS", guid);
            string sksMountDir = Path.Combine(MountDir, "SKS");
            string appTPMpath = Path.Combine(sksMountDir, guid);
            string keyPath = Path.Combine(appTPMpath, "keys");
            byte[] key;
            if (File.Exists(Path.Combine(keyPath, "AES_MASTER_KEY_IMPORTANT_DO_NOT_DELETE")))
            {
                key = TPMRetrieveKeySecurely("AES_MASTER_KEY_IMPORTANT_DO_NOT_DELETE");
            } 
            else
            {
                key = Rfc2898DeriveBytes.Pbkdf2(GenerateRandomBytes(32), GenerateRandomBytes(32), 10000, HashAlgorithmName.SHA256, AesKeySize);
            }
            byte[] nonce = Rfc2898DeriveBytes.Pbkdf2(GenerateRandomBytes(16), GenerateRandomBytes(16), 1000, HashAlgorithmName.SHA256, AesNonceSize);
            byte[] tag = new byte[AesTagSize];
            byte[] plaintext = File.ReadAllBytes(inputFile);
            byte[] ciphertext = new byte[plaintext.Length];
            using (var outputStream = File.Create(outputFile))
            {

                using (var aes = new AesGcm(key, 16))
                {
                    aes.Encrypt(nonce, plaintext, ciphertext, tag);
                }

            }
            // Create the JSON structure
            EncryptedData encryptedDataJsonFormat = new EncryptedData
            {
                Nonce = Convert.ToBase64String(nonce),
                CipherText = Convert.ToBase64String(ciphertext),
                Tag = Convert.ToBase64String(tag),
                TagBase64Hash = ComputeHash(Encoding.UTF8.GetBytes(Convert.ToBase64String(tag)), HashingAlgorithm.SHA256),
                TagHash = ComputeHash(tag, HashingAlgorithm.SHA256),
                Timestamp = DateTime.Now.ToString()
            };

            string jsonString = JsonConvert.SerializeObject(encryptedDataJsonFormat);
            File.WriteAllText(outputFile, jsonString);
            TPMStoreKeySecurely(key, "AES_MASTER_KEY_IMPORTANT_DO_NOT_DELETE");

            Array.Clear(plaintext);
            Array.Clear(ciphertext);
            Array.Clear(tag);
            Array.Clear(key);
            Array.Clear(nonce);
        }

        public static void DecryptFile(string inputFile, string outputFile)
        {
            if (!File.Exists(inputFile))
            {
                throw new FileNotFoundException(inputFile + " Cannot be decrypted as it is cannot be found!");
            }
            byte[] key = TPMRetrieveKeySecurely("AES_MASTER_KEY_IMPORTANT_DO_NOT_DELETE");
            string json = File.ReadAllText(inputFile);

            EncryptedData? encryptedDataInformation = JsonConvert.DeserializeObject<EncryptedData>(json);

            if (encryptedDataInformation == null)
            {
                throw new FormatException("The file cannot be decrypted as it is not in the correct format.");
            }
            if (encryptedDataInformation.Tag == null)
            {
                throw new FormatException("The file cannot be decrypted as it is not in the correct format.");
            }
            if (encryptedDataInformation.CipherText == null)
            {
                throw new FormatException("The file cannot be decrypted as it is not in the correct format.");
            }
            if(encryptedDataInformation.Nonce == null)
            {
                throw new FormatException("The file cannot be decrypted as it is not in the correct format.");
            }
            if(encryptedDataInformation.TagBase64Hash == null)
            {
                throw new FormatException("The file cannot be decrypted as it is not in the correct format.");
            }
            if (encryptedDataInformation.TagHash == null)
            {
                throw new FormatException("The file cannot be decrypted as it is not in the correct format.");
            }
            if (encryptedDataInformation.Timestamp == null)
            {
                throw new FormatException("The file cannot be decrypted as it is not in the correct format.");
            }

            byte[] nonce = Convert.FromBase64String(encryptedDataInformation.Nonce);
            byte[] ciphertext = Convert.FromBase64String(encryptedDataInformation.CipherText);
            byte[] tag = Convert.FromBase64String(encryptedDataInformation.Tag);

            string ComputedTagBase64HashNow = ComputeHash(Encoding.UTF8.GetBytes(encryptedDataInformation.Tag), HashingAlgorithm.SHA256);
            string ComputedTagHashNow = ComputeHash(tag, HashingAlgorithm.SHA256);

            if (ComputedTagBase64HashNow != encryptedDataInformation.TagBase64Hash)
            {
                throw new CryptographicException("The tag hash from the file and the computed hash doesnt match!");
            }

            if (ComputedTagHashNow != encryptedDataInformation.TagHash)
            {
                throw new CryptographicException("The tag hash from the file and the computed hash doesnt match!");
            }


            byte[] plaintext = new byte[ciphertext.Length];
            using (var outputStream = File.Create(outputFile))
            {

                using (var aes = new AesGcm(key, 16))
                {
                    aes.Decrypt(nonce, ciphertext, tag, plaintext);
                }
            }
            File.WriteAllBytes(outputFile, plaintext);
            Array.Clear(plaintext);
            Array.Clear(ciphertext);
            Array.Clear(tag);
            Array.Clear(key);
            Array.Clear(nonce);
        }

        //Little Out of place is it :)
        public static string TarDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException(directoryPath + " not found!");
            }
            string tarFilePath = directoryPath + ".tar";

            // Create a tar file
            using (FileStream tarFileStream = File.Create(tarFilePath))
            {
                TarFile.CreateFromDirectory(directoryPath, tarFileStream, false);
                tarFileStream.Close();
            }


            return tarFilePath;
        }


        public static void GzipFile(string inputFilePath, string outputFilePath)
        {
            if (string.IsNullOrEmpty(inputFilePath))
            {
                throw new ArgumentException($"'{nameof(inputFilePath)}' cannot be null or empty.", nameof(inputFilePath));
            }

            if (string.IsNullOrEmpty(outputFilePath))
            {
                throw new ArgumentException($"'{nameof(outputFilePath)}' cannot be null or empty.", nameof(outputFilePath));
            }

            if (!File.Exists(inputFilePath))
            {
                throw new FileNotFoundException(inputFilePath + " not found!");
            }
            FileAttributes atrr = File.GetAttributes(inputFilePath);
            if ((atrr != FileAttributes.None) && atrr.HasFlag(FileAttributes.Directory))
            {
                throw new NotSupportedException("Currently GZIP-ing a folder directly is not supported");
            }
            using (FileStream inputFileStream = File.OpenRead(inputFilePath))
            using (FileStream outputFileStream = File.Create(outputFilePath))
            using (GZipStream gzipStream = new GZipStream(outputFileStream, CompressionMode.Compress))
            {
                inputFileStream.CopyTo(gzipStream);
            }
        }

    }
}

