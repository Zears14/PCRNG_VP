using System;
using System.Security.Cryptography;

namespace PCRNG_VP.exTPM.Cryptography.HashingProvider
{
    public class Sha512 : IHashingProvider
    {
        public Sha512() { }

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
