using System;
using System.Linq;
using System.Text;

namespace PresetMagician.Utils
{
    public static class StringUtils
    {
        public static string ByteArrayToHexString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        
        public static string Int32ToHexString(int value)
        {
            var values = BitConverter.GetBytes(value).Select(x => x.ToString("x2")).Reverse();
            
            return string.Join(" ", values);
        }
    }
}