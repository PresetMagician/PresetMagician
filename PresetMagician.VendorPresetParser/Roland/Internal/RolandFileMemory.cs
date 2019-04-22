using System.IO;

namespace PresetMagician.VendorPresetParser.Roland.Internal
{
    public class RolandFileMemory
    {
        private byte[] FileData;
        
        public void SetFileData(byte[] data)
        {
            FileData = data;
        }

        public byte[] GetFromFile(int address, int length)
        {
            
            if (FileData == null)
            {
                return null;
            }
            if (address +length> FileData.Length)
            {
                return null;
            }
            
            var b = new byte[length];
            for (var i = 0; i < length; i++)
            {
                b[i] = FileData[address + i];
            }

            return b;
        }
        
        public void ReadFile(string file)
        {
            if (!File.Exists(file))
            {
                return;
            }
            FileData = File.ReadAllBytes(file);
        }
    }
}