using System.Security.Cryptography;

namespace PCRNG_VP.exTPM.Helpers.Cryptography
{
    public static class EntropySource
    {
        public static byte[] GenerateRandomBytes(int length)
        {
            byte[] randomBytes = new byte[length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
