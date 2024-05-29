using Force.Crc32;
using System;

namespace PCRNG_VP.exTPM.Cryptography.HashingProvider.UnsecureHash
{
    /// <summary>
    /// Provides CRC32 hashing functionality.
    /// </summary>
    public class Crc32 : IHashingProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Crc32"/> class.
        /// </summary>
        public Crc32() { }

        /// <summary>
        /// Computes the CRC32 hash of the given data.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>A string representing the hexadecimal format of the CRC32 hash.</returns>
        public string ComputeHash(byte[] data)
        {
            uint hash = Crc32Algorithm.Compute(data);
            return BitConverter.ToString(BitConverter.GetBytes(hash)).Replace("-", "").ToLower(); // Convert hash bytes to lowercase hexadecimal string
        }
    }
}
