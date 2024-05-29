using PCRNG_VP.exTPM.Cryptography.KeyStoreProvider;
using PCRNG_VP.exTPM.Helpers.Cryptography;
using PCRNG_VP.exTPM.IO.CustomFileFormat;
using System;
using System.IO;
using System.Security.Cryptography;
using static PCRNG_VP.exTPM.Helpers.Cryptography.EntropySource;

namespace PCRNG_VP.exTPM.Cryptography.CipherProvider
{
    /// <summary>
    /// Provides AES-GCM encryption and decryption with PVEF encoding.
    /// </summary>
    public class AesGcm : ICipherProvider
    {
        private readonly string _baseDir = Path.Combine(Directory.GetCurrentDirectory(), "FS", Helpers.AppGuidRetrieval.GetAppGuid());

        /// <summary>
        /// Encrypts data and encodes it in PVEF format.
        /// </summary>
        /// <param name="dataToBeEncrypted">The data to be encrypted.</param>
        /// <returns>A byte array containing the encrypted data in PVEF format.</returns>
        public byte[] EncryptToPVEFEncoded(byte[] dataToBeEncrypted)
        {
            byte[] key = GetAesMasterKey.GetMasterKey(_baseDir);
            using (var aesGcm = new System.Security.Cryptography.AesGcm(key, 16))
            {
                byte[] cipherText = new byte[dataToBeEncrypted.Length];
                byte[] aesTag = new byte[16];
                byte[] nonce = Rfc2898DeriveBytes.Pbkdf2(GenerateRandomBytes(16), GenerateRandomBytes(16), 1000, HashAlgorithmName.SHA256, 16);

                aesGcm.Encrypt(nonce, dataToBeEncrypted, cipherText, aesTag);

                Pvef pvefEncoder = new Pvef();
                byte[] pvefEncoded = pvefEncoder.Encode(cipherText, nonce, aesTag);

                Array.Clear(nonce);
                Array.Clear(dataToBeEncrypted);
                Array.Clear(key);

                return pvefEncoded;
            }
        }

        /// <summary>
        /// Decrypts data from PVEF format.
        /// </summary>
        /// <param name="dataToBeDecrypted">The data to be decrypted.</param>
        /// <returns>A byte array containing the decrypted data.</returns>
        public byte[] DecryptFromPVEF(byte[] dataToBeDecrypted)
        {
            Pvef pvefDecoder = new Pvef();
            byte[][] parsedData = pvefDecoder.Parse(dataToBeDecrypted);

            byte[] ciphertext = parsedData[0];
            byte[] nonce = parsedData[1];
            byte[] tag = parsedData[2];
            byte[] key = GetAesMasterKey.GetMasterKey(_baseDir);

            using (var aesGcm = new System.Security.Cryptography.AesGcm(key, 16))
            {
                byte[] decryptedData = new byte[ciphertext.Length];
                aesGcm.Decrypt(nonce, ciphertext, tag, decryptedData);

                Array.Clear(ciphertext);
                Array.Clear(nonce);
                Array.Clear(tag);
                Array.Clear(key);

                return decryptedData;
            }
        }
    }
}
