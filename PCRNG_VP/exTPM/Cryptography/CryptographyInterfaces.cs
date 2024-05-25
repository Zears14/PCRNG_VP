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
    public interface ICipherProvider
    {
        public abstract byte[] EncryptToPVEFEncoded(byte[] dataTobeEncrypted);
        public abstract byte[] DecryptFromPVEF(byte[] dataTobeDecrypted);
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
