using System.Security.Cryptography;

namespace PCRNG_VP.exTPM.Helpers.Cryptography
{
    /// <summary>
    /// Provides methods to generate random bytes.
    /// </summary>
    public static class EntropySource
    {
        /// <summary>
        /// Generates an array of random bytes.
        /// </summary>
        /// <param name="length">The length of the byte array to generate.</param>
        /// <returns>An array of random bytes.</returns>
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
