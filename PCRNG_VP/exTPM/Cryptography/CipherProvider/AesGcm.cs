using PCRNG_VP.exTPM.Cryptography.KeyStoreProvider;
using PCRNG_VP.exTPM.Helpers.Cryptography;
using PCRNG_VP.exTPM.IO.CustomFIleFormat;
using System;
using System.IO;
using System.Security.Cryptography;
using static PCRNG_VP.exTPM.Helpers.Cryptography.EntropySource;

namespace PCRNG_VP.exTPM.Cryptography.CipherProvider
{
    public class AesGcm : ICipherProvider
    {
        private readonly string _baseDir = Path.Combine(Directory.GetCurrentDirectory(), "FS", Helpers.AppGuidRetrieval.GetAppGuid());
        public byte[] EncryptToPVEFEncoded(byte[] dataTobeEncrypted)
        {
            byte[] key = GetAesMasterKey.GetMasterKey(_baseDir);
            using (var aesGcm = new System.Security.Cryptography.AesGcm(key, 16))
            {
                byte[] cipherText = new byte[dataTobeEncrypted.Length];
                byte[] aesTag = new byte[16];
                byte[] nonce = Rfc2898DeriveBytes.Pbkdf2(GenerateRandomBytes(16), GenerateRandomBytes(16), 1000, HashAlgorithmName.SHA256, 16);
                aesGcm.Encrypt(nonce, dataTobeEncrypted, cipherText, aesTag);
                Pvef pvefEncoder = new Pvef();
                byte[] pvefEncoded = pvefEncoder.Encode(cipherText, nonce, aesTag);
                Array.Clear(nonce);
                Array.Clear(dataTobeEncrypted);
                Array.Clear(key);
                return pvefEncoded;
            }
        }

        public byte[] DecryptFromPVEF(byte[] dataTobeDecrypted)
        {
            Pvef pvefDecoder = new Pvef();
            byte[][] parsedData = pvefDecoder.Parse(dataTobeDecrypted);
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
