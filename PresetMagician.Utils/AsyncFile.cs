using System.IO;
using System.Threading.Tasks;

namespace PresetMagician.Utils
{
    public class AsyncFile
    {
        public static async Task<byte[]> ReadAllBytesAsync(string file)
        {
            byte[] result;
            using (FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read))
            {
                result = new byte[stream.Length];
                await stream.ReadAsync(result, 0, (int) stream.Length);
            }

            return result;
        }
    }
}