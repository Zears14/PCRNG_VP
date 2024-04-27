using Force.Crc32;
using System;

namespace PCRNG_VP.exTPM.Cryptography.Hash.Unsecure
{
    public class Crc32HashProvider : IHashingProvider
    {
        public Crc32HashProvider() { }
        public string ComputeHash(byte[] data)
        {
            uint hash = Crc32Algorithm.Compute(data);
            return BitConverter.ToString(BitConverter.GetBytes(hash));
        }
    }
}
