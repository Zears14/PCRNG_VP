using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCRNG_VP.exTPM.IO.CustomFIleFormat
{
    public class Pvef : ICustomFileFormat
    {
        private const string Header = "PVEF";
        private const byte Version = 1;
        private const int NonceSize = 16;
        private const int TagSize = 16;
        public byte[] Encode(params byte[][] data)
        {
            if (data.Length != 3)
            {
                throw new ArgumentException("Encode requires exactly three byte arrays: ciphertext, nonce, and tag.");
            }

            byte[] ciphertext = data[0];
            byte[] nonce = data[1];
            byte[] tag = data[2];
            if (nonce.Length != NonceSize)
            {
                throw new ArgumentException($"Nonce must be {NonceSize} bytes.");
            }
            if (tag.Length != TagSize)
            {
                throw new ArgumentException($"Tag must be {TagSize} bytes.");
            }
            using (var memoryStream = new MemoryStream())
            {
                // Write the header
                memoryStream.Write(Encoding.ASCII.GetBytes(Header), 0, 4);

                // Write the version
                memoryStream.WriteByte(Version);

                // Write the reserved bytes
                memoryStream.Write(new byte[3], 0, 3);

                // Write the ciphertext
                memoryStream.Write(ciphertext, 0, ciphertext.Length);

                // Write the nonce
                memoryStream.Write(nonce, 0, NonceSize);

                // Write the tag
                memoryStream.Write(tag, 0, TagSize);

                return memoryStream.ToArray();
            }
        }
        public byte[][] Parse(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    // Read and validate the header
                    string header = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
                    if (header != Header)
                    {
                        throw new InvalidDataException("Invalid file header.");
                    }

                    // Read and validate the version
                    byte version = binaryReader.ReadByte();
                    if (version != Version)
                    {
                        throw new InvalidDataException("Unsupported version.");
                    }

                    // Skip the reserved bytes
                    binaryReader.ReadBytes(3);

                    // Calculate the length of the ciphertext
                    int ciphertextLength = (int)(memoryStream.Length - memoryStream.Position - NonceSize - TagSize);

                    // Read the ciphertext
                    byte[] ciphertext = binaryReader.ReadBytes(ciphertextLength);

                    // Read the nonce
                    byte[] nonce = binaryReader.ReadBytes(NonceSize);

                    // Read the tag
                    byte[] tag = binaryReader.ReadBytes(TagSize);

                    return new byte[][] { ciphertext, nonce, tag };
                }
            }
        }
    }
}
