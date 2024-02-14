using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Formats.Tar;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PCRNG_VP.exTPM
{
    public static class Crypto
    {
        public static byte[] GenerateRandomBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
        }

        public static string TarDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                throw new FileNotFoundException(directoryPath+" not found!");
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
            using (FileStream inputFileStream = File.OpenRead(inputFilePath))
            using (FileStream outputFileStream = File.Create(outputFilePath))
            using (GZipStream gzipStream = new GZipStream(outputFileStream, CompressionMode.Compress))
            {
                inputFileStream.CopyTo(gzipStream);
            }
        }
    }
}
