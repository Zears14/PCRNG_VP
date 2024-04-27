using System;
using System.Security.Cryptography;

namespace PCRNG_VP.exTPM.Cryptography.Hash
{
    public class Sha512HashProvider : IHashingProvider
    {
        public Sha512HashProvider() { }

        public string ComputeHash(byte[] data)
        {
            using (SHA512 hasher = SHA512.Create())
            {
                byte[] hashBytes = hasher.ComputeHash(data);
                return BitConverter.ToString(hashBytes); // Convert hash bytes to hexadecimal string
            }
        }
    }
}
