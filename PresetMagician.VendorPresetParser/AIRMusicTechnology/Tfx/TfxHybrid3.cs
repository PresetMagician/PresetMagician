using System.IO;
using Drachenkatze.PresetMagician.VendorPresetParser.Properties;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxHybrid3 : Tfx
    {
        public override byte[] WzooPluginId { get; }=  {0x54, 0x68, 0x75, 0x6e};
        public override byte[] BlockMagic { get; }=  {0x00, 0x00, 0x1f, 0xd6};
        
        
        public override void PostProcess()
        {
            // replace filename in metadata
            
            /*var ms2 = new MemoryStream(WzooBlock.BlockData);
            ms2.Seek(5556, SeekOrigin.Begin); 

            var pos = ms2.Position;
            var end = 0;
            var found = true;
            while (true)
            {
                if (ms2.ReadByte() == 0)
                {
                    break;
                }

                if (end + pos >= ms2.Length)
                {
                    found = false;
                    break;
                }

                end++;
            }

            if (found)
            {
                for (var i = pos; i < pos + end; i++)
                {
                    WzooBlock.BlockData[i] = 0;
                }

                for (var i = 0; i < PatchName.Length; i++)
                {
                    WzooBlock.BlockData[pos + i] = PatchName[i];
                }
            }*/

            EndChunk = Resource1.Hybrid3EndChunk;
        }
    }

}