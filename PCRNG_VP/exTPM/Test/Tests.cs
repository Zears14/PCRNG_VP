using Force.Crc32;
using PCRNG_VP.exTPM.Cryptography;
using Spectre.Console;
using System.Diagnostics;
using System.IO;
using static PCRNG_VP.exTPM.TPMLogger;
namespace PCRNG_VP.exTPM.Test
{
    public static class Tests
    {
        public static void EncryptDecryptVerifyTest()
        {
            string CurrentDir = Directory.GetCurrentDirectory();

            int[] fileSizes = { 1024, 1024 * 1024, 1024 * 1024 * 1024 }; // 1KB, 1MB, 1GB

            if (AnsiConsole.Confirm("This test will encrypt and decrypt files up to 1GB in size. Continue?"))
            {
                foreach (int fileSize in fileSizes)
                {
                    Log($"Testing Encryption for file size: {fileSize} bytes", extra: ": DEBUG-TIME-POST-TPMSETUP", severity: LogLevel.DEBUG);

                    string testFile = Path.Combine(CurrentDir, $"DEBUG_TEST_FILE_{fileSize}");
                    string encryptedFile = Path.Combine(CurrentDir, $"DEBUG_TEST_FILE_{fileSize}_ENCRYPTED");

                    byte[] randomData = Crypto.GenerateRandomBytes(fileSize);
                    File.WriteAllBytes(testFile, randomData);

                    var stopwatch = Stopwatch.StartNew();
                    Crypto.EncryptFile(testFile, encryptedFile);
                    stopwatch.Stop();
                    Log($"Encryption time for {fileSize} bytes: {stopwatch.ElapsedMilliseconds} ms", extra: ": DEBUG-TIME-POST-TPMSETUP", severity: LogLevel.DEBUG);

                    stopwatch.Restart(); // Reuse the stopwatch 
                    Crypto.DecryptFile(encryptedFile, testFile + "_DECRYPTED");
                    stopwatch.Stop();
                    Log($"Decryption time for {fileSize} bytes: {stopwatch.ElapsedMilliseconds} ms", extra: ": DEBUG-TIME-POST-TPMSETUP", severity: LogLevel.DEBUG);

                    bool validationResult = CompareFilesWithCRC32(testFile, testFile + "_DECRYPTED");
                    Log(validationResult ? "Encryption/Decryption SUCCESSFUL" : "Encryption/Decryption FAILED", extra: ": DEBUG-TIME-POST-TPMSETUP", severity: LogLevel.DEBUG);
                }
            }
            else
            {
                Log("Test canceled by user.", extra: ": DEBUG-TIME-POST-TPMSETUP", severity: LogLevel.INFO);
            }

            static bool CompareFilesWithCRC32(string file1, string file2)
            {
                byte[] File1Content = File.ReadAllBytes(file1);
                byte[] File2Content = File.ReadAllBytes(file2);
                uint File1Crc = Crc32Algorithm.Compute(File1Content);
                uint File2Crc = Crc32Algorithm.Compute(File2Content);
                return File1Crc == File2Crc;
            }
            
        }
    }
}
