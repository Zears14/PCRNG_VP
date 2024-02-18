using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCRNG_VP.exTPM
{
    public class KeyMetadata
    {
        public  Types.KeyType KeyType { get; set; } // Symmetric or Asymmetric
        public  Types.KeyAlgorithm KeyAlgorithm { get; set; } // AES or RSA
        public  int? KeySize { get; set; } // The key size
        public  string? KeyFingerprint { get; set; } //SHA256 Representation of the key

        public KeyMetadata()
        {

        }
    }
}
