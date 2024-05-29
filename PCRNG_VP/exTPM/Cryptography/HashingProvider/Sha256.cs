using System;
using System.Security.Cryptography;

namespace PCRNG_VP.exTPM.Cryptography.HashingProvider
{
    /// <summary>
    /// Provides SHA-256 hashing functionality.
    /// </summary>
    public class Sha256 : IHashingProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sha256"/> class.
        /// </summary>
        public Sha256() { }

        /// <summary>
        /// Computes the SHA-256 hash of the given data.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>A string representing the hexadecimal format of the SHA-256 hash.</returns>
        public string ComputeHash(byte[] data)
        {
            using (SHA256 hasher = SHA256.Create())
            {
                byte[] hashBytes = hasher.ComputeHash(data);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower(); // Convert hash bytes to lowercase hexadecimal string
            }
        }
    }
}
