using System.IO;
using Catel.IO;
using GSF;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class MagicBlock
    {
        public byte[] BlockMagic;
        public byte[] PluginName;
        public byte[] BlockData;
        public bool IsMagicBlock = true;

        public byte[] GetDataToWrite()
        {
            if (IsMagicBlock)
            {
                var length = BlockMagic.Length + // block magic
                             4 + // plugin name length
                             PluginName.Length + // plugin name itself
                             BlockData.Length;

                using (var ms = new MemoryStream())
                {
                    ms.Write(LittleEndian.GetBytes(length), 0, 4);
                    ms.Write(BlockMagic, 0, BlockMagic.Length);
                    ms.Write(BigEndian.GetBytes(PluginName.Length), 0, 4);
                    ms.Write(PluginName, 0, PluginName.Length);
                    ms.Write(BlockData, 0, BlockData.Length);

                    return ms.ToByteArray();
                }
            }

            using (var ms = new MemoryStream())
            {
                var length2 = BlockData.Length;
                ms.Write(LittleEndian.GetBytes(length2), 0, 4);
                ms.Write(BlockData, 0, BlockData.Length);
                ms.Write(new byte[] {0xDE, 0xAD, 0xBE, 0xEF},0,4);

                return ms.ToByteArray();
            }
        }

    }
}