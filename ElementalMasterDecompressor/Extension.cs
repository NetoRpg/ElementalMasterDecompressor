using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ElementalMasterDecompressor
{
    static class Extension
    {

        public static int GetInt32(this byte[] bytes, int index = 0)
        {
            return (bytes[index + 0] << 24) + (bytes[index + 1] << 16) + (bytes[index + 2] << 8) + bytes[index + 3];
        }

        public static byte[] extractPiece(this MemoryStream ms, int offset, int length, int changeOffset = -1)
        {
            if (changeOffset > -1) ms.Position = changeOffset;

            byte[] data = new byte[length];
            ms.Read(data, 0, length);

            return data;
        }


    }
}
