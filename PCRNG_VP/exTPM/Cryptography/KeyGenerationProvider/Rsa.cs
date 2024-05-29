using System.Security.Cryptography;
using System.Text;

namespace PCRNG_VP.exTPM.Cryptography.KeyGeneration
{
    /// <summary>
    /// Provides RSA key generation functionality.
    /// </summary>
    public class Rsa : IAsymmetricKeyGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rsa"/> class with the specified key size.
        /// </summary>
        /// <param name="KeySize">The size of the RSA key to generate.</param>
        public Rsa(int KeySize)
        {
            AlgorithmName = "RSA";
            this.KeySize = KeySize;
        }

        /// <summary>
        /// Gets the name of the algorithm.
        /// </summary>
        public string AlgorithmName { get; }

        /// <summary>
        /// Gets the size of the key.
        /// </summary>
        public int KeySize { get; }

        /// <summary>
        /// Generates an RSA key pair and encrypts the private key using the specified password.
        /// </summary>
        /// <param name="password">The password to encrypt the private key.</param>
        /// <returns>The encrypted private key in PEM format.</returns>
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
