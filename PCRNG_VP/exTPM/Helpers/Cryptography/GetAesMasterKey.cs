using PCRNG_VP.exTPM.Cryptography.KeyStoreProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCRNG_VP.exTPM.Helpers.Cryptography
{
    public static class GetAesMasterKey
    {
        public static byte[] GetMasterKey(string _baseDir)
        {
            ExternalKeyStore externalKeyStore = new ExternalKeyStore(_baseDir);
            return externalKeyStore.RetrieveKey("AES_MASTER_KEY_IMPORTANT_DO_NOT_DELETE");
        }
    }
}
