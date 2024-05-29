using System;
using System.Security.Cryptography;

namespace PCRNG_VP.exTPM.Cryptography.HashingProvider
{
    /// <summary>
    /// Provides SHA-512 hashing functionality.
    /// </summary>
    public class Sha512 : IHashingProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sha512"/> class.
        /// </summary>
        public Sha512() { }

        /// <summary>
        /// Computes the SHA-512 hash of the given data.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>A string representing the hexadecimal format of the SHA-512 hash.</returns>
        public string ComputeHash(byte[] data)
        {
            using (SHA512 hasher = SHA512.Create())
            {
                byte[] hashBytes = hasher.ComputeHash(data);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower(); // Convert hash bytes to lowercase hexadecimal string
            }
        }
    }
}
