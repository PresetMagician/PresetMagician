using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Catel.Collections;
using GSF.Collections;
using GSF.IO;

namespace PresetMagician.Utils
{
    public static class StreamUtils
    {
        /// <summary>
        /// Reads the specified number of bytes and returns the resulting string.
        /// </summary>
        /// <param name="stream">The stream to write to</param>
        /// <param name="length">The length to read</param>
        /// <returns></returns>
        public static string ReadNullTerminatedString(this Stream stream, int length)
        {
            var buf = stream.ReadBytes(length);
            var str = Encoding.UTF8.GetString(buf);
            var firstNull = str.IndexOf((char)0x00);

            if (firstNull == -1)
            {
                return str;
            }

            return str.Substring(0, firstNull);
        }

        public static void WriteNullTerminatedString(this Stream stream, string str, int length)
        {
            var strBytes = Encoding.UTF8.GetBytes(str);

            if (strBytes.Length > length)
            {
                strBytes = strBytes.GetRange(0, length - 2).ToArray();
            }

            var lengthToPad = length - strBytes.Length;
            var pad = new byte[lengthToPad];
            strBytes = strBytes.Concat(pad).ToArray();
            
            stream.Write(strBytes,0, length);
        }
    }
}