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

namespace PCRNG_VP.exTPM
{
    public static class Crypto
    {
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

        //Little Out of place is it :)
        public static string TarDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException(directoryPath+" not found!");
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
