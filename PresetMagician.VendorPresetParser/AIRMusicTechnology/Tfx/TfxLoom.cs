using System.IO;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxLoom : Tfx
    {
        public override byte[] BlockMagic { get; } = {0x05, 0xf7, 0x3c, 0xa8};

        public override void PostProcess()
        {
            var ms2 = new MemoryStream(WzooBlock.BlockData);
            ms2.Seek(12 + 640, SeekOrigin.Begin);

            var pos = ms2.Position;
            var end = 0;
            while (true)
            {
                if (ms2.ReadByte() == 0)
                {
                    break;
                }

                end++;
            }

            for (var i = pos; i < pos + end; i++)
            {
                WzooBlock.BlockData[i] = 0;
            }

            for (var i = 0; i < PatchName.Length; i++)
            {
                WzooBlock.BlockData[pos + i] = PatchName[i];
            }
        }

        public override byte[] GetEndChunk()
        {
            return VendorResources.LoomClassicEndChunk;
        }
    }
}