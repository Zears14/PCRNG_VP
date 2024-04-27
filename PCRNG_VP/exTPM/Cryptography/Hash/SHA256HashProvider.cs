using System;
using System.Security.Cryptography;

namespace PCRNG_VP.exTPM.Cryptography.Hash
{
    public class Sha256HashProvider : IHashingProvider
    {
        public Sha256HashProvider() { }
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
