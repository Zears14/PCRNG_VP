using Force.Crc32;
using System;

namespace PCRNG_VP.exTPM.Cryptography.HashingProvider.UnsecureHash
{
    public class Crc32 : IHashingProvider
    {
        public Crc32() { }
        public string ComputeHash(byte[] data)
        {
            uint hash = Crc32Algorithm.Compute(data);
            return BitConverter.ToString(BitConverter.GetBytes(hash));
        }
    }
}
