using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static PCRNG_VP.exTPM.Types;

namespace PCRNG_VP.exTPM.Cryptography
{
    public interface IKeyGenerationProvider
    {
        abstract string AlgorithmName { get; }
        abstract int KeySize { get; }
    }

    public interface IAsymmetricKeyGenerator : IKeyGenerationProvider
    {
        abstract string GenerateKeyPairToEncryptedPEMEncoded(string password);
    }

    public interface ISymmetricKeyGenerator : IKeyGenerationProvider
    {

    }
    public interface IEncryptionProvider
    {
        public abstract byte[] Encrypt(byte[] dataTobeEncrypted);
        public abstract byte[] Decrypt(byte[] dataTobeDecrypted);
    }
    public interface IHashingProvider
    {
        public abstract string ComputeHash(byte[] data);
    }
    public interface IKeyStoreProvider
    {
        public abstract byte[] RetrieveKey(string KeyName);
        public abstract void StoreKey(string KeyName, byte[] rawKeyBytes);
    }
}
