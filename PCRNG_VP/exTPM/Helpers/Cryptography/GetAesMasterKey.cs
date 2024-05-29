using PCRNG_VP.exTPM.Cryptography.KeyStoreProvider;
using System;

namespace PCRNG_VP.exTPM.Helpers.Cryptography
{
    /// <summary>
    /// Provides a helper method to retrieve the AES master key.
    /// </summary>
    public static class GetAesMasterKey
    {
        /// <summary>
        /// Retrieves the AES master key from the external key store.
        /// </summary>
        /// <param name="_baseDir">The base directory of the key store.</param>
        /// <returns>The AES master key as a byte array.</returns>
        public static byte[] GetMasterKey(string _baseDir)
        {
            ExternalKeyStore externalKeyStore = new ExternalKeyStore(_baseDir);
            return externalKeyStore.RetrieveKey("AES_MASTER_KEY_IMPORTANT_DO_NOT_DELETE");
        }
    }
}
