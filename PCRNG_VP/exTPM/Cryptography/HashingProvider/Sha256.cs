using System;
using System.Security.Cryptography;

namespace PCRNG_VP.exTPM.Cryptography.HashingProvider
{
    public class Sha256 : IHashingProvider
    {
        public Sha256() { }
        public string ComputeHash(byte[] data)
        {
            using (SHA256 hasher = SHA256.Create())
            {
                byte[] hashBytes = hasher.ComputeHash(data);
                return BitConverter.ToString(hashBytes); // Convert hash bytes to hexadecimal string
            }
        }
    }
}
