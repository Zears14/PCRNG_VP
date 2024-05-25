using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCRNG_VP.exTPM.IO
{
    interface ICustomFileFormat
    {
        virtual byte[][] Parse(byte[] data) { throw new NotImplementedException(); }
        abstract byte[] Encode(params byte[][] data);
    }
}
