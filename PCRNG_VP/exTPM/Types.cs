using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PCRNG_VP.exTPM
{
    public class Types
    {
        public enum KeyType
        {
            Symmetric,
            Asymmetric
        }

        public enum KeyAlgorithm
        {
            AES,
            RSA
        }

        public enum HashingAlgorithm
        {
            SHA256,
            SHA512
        }

        public class EncryptedData
        {
            public string? Nonce { get; set; }
            public string? CipherText { get; set; }
            public string? Tag { get; set; }
            public string? TagBase64Hash { get; set; }
            public string? TagHash { get; set; }
            public string? Timestamp { get; set; }
        }
    }
}
