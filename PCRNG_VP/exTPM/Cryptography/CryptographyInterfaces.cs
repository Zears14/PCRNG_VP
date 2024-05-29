namespace PCRNG_VP.exTPM.Cryptography
{
    /// <summary>
    /// The root interface for all key generation providers.
    /// </summary>
    public interface IKeyGenerationProvider
    {
        /// <summary>
        /// Gets the name of the algorithm used for key generation.
        /// </summary>
        string AlgorithmName { get; }

        /// <summary>
        /// Gets the size of the key to be generated.
        /// </summary>
        int KeySize { get; }
    }

    /// <summary>
    /// Interface for generating asymmetric keys.
    /// </summary>
    /// <seealso cref="PCRNG_VP.exTPM.Cryptography.IKeyGenerationProvider" />
    public interface IAsymmetricKeyGenerator : IKeyGenerationProvider
    {
        /// <summary>
        /// Generates an asymmetric key pair and returns it as an encrypted PEM-encoded string.
        /// </summary>
        /// <param name="password">The password to encrypt the key pair.</param>
        /// <returns>The encrypted PEM-encoded key pair.</returns>
        string GenerateKeyPairToEncryptedPEMEncoded(string password);
    }

    /// <summary>
    /// Interface for generating symmetric keys. Work in progress.
    /// </summary>
    /// <seealso cref="PCRNG_VP.exTPM.Cryptography.IKeyGenerationProvider" />
    public interface ISymmetricKeyGenerator : IKeyGenerationProvider
    {
        // Interface definition pending.
    }

    /// <summary>
    /// Interface for implementing cipher algorithms.
    /// </summary>
    /// <seealso cref="PCRNG_VP.exTPM.IO.CustomFileFormat.Pvef" />
    public interface ICipherProvider
    {
        /// <summary>
        /// Encrypts data and returns it in PVEF format.
        /// </summary>
        /// <param name="dataToBeEncrypted">The data to be encrypted.</param>
        /// <returns>The encrypted data in PVEF format.</returns>
        byte[] EncryptToPVEFEncoded(byte[] dataToBeEncrypted);

        /// <summary>
        /// Decrypts data from PVEF format.
        /// </summary>
        /// <param name="dataToBeDecrypted">The data to be decrypted.</param>
        /// <returns>The decrypted data.</returns>
        byte[] DecryptFromPVEF(byte[] dataToBeDecrypted);
    }

    /// <summary>
    /// Interface for implementing hashing algorithms.
    /// </summary>
    public interface IHashingProvider
    {
        /// <summary>
        /// Computes the hash of the given data.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The computed hash as a string.</returns>
        string ComputeHash(byte[] data);
    }

    /// <summary>
    /// Interface for key storage providers.
    /// </summary>
    public interface IKeyStoreProvider
    {
        /// <summary>
        /// Retrieves a key by name.
        /// </summary>
        /// <param name="keyName">The name of the key to retrieve.</param>
        /// <returns>The raw key bytes.</returns>
        byte[] RetrieveKey(string keyName);

        /// <summary>
        /// Stores a key with the given name.
        /// </summary>
        /// <param name="keyName">The name of the key to store.</param>
        /// <param name="rawKeyBytes">The raw key bytes to store.</param>
        void StoreKey(string keyName, byte[] rawKeyBytes);
    }
}
