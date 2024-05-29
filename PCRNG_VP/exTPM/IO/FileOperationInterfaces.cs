using System;

namespace PCRNG_VP.exTPM.IO
{
    /// <summary>
    /// Defines methods for custom file format encoding and parsing.
    /// </summary>
    interface ICustomFileFormat
    {
        /// <summary>
        /// Parses the given byte array into its components.
        /// </summary>
        /// <param name="data">The byte array to parse.</param>
        /// <returns>An array of byte arrays containing the parsed components.</returns>
        /// <exception cref="NotImplementedException">Thrown if the method is not implemented by the derived class.</exception>
        virtual byte[][] Parse(byte[] data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Encodes the given components into a byte array.
        /// </summary>
        /// <param name="data">An array of byte arrays to encode.</param>
        /// <returns>A byte array representing the encoded data.</returns>
        /// <exception cref="NotImplementedException">Thrown if the method is not implemented by the derived class.</exception>
        abstract byte[] Encode(params byte[][] data);
    }
}
