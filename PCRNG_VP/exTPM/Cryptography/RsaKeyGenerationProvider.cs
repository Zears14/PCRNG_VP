using System.Security.Cryptography;
using System.Text;

namespace PCRNG_VP.exTPM.Cryptography
{
    public class RsaKeyGenerationProvider : IAsymmetricKeyGenerator
    {
        public RsaKeyGenerationProvider(int KeySize)
        {
            this.AlgorithmName = "RSA";
            this.KeySize = KeySize;
        }
        public string AlgorithmName { get; }
        public int KeySize { get; }

        public string GenerateKeyPairToEncryptedPEMEncoded(string password)
        {
            using (RSA rsa = RSA.Create())
            {

                rsa.KeySize = KeySize;
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                PbeParameters pbeParameters = new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA512, 10000);

                string key = rsa.ExportEncryptedPkcs8PrivateKeyPem(passwordBytes, pbeParameters);
                return key;
            }
        }

    }
}
