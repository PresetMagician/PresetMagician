using System.IO;
using Catel.IO;
using GSF;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxVacuumPro : Tfx
    {
        public override byte[] BlockMagic { get; } = {0x05, 0xf7, 0x3c, 0xa8};
        
        // Note: Reverted to false and added the /Default patch name
        // Expansions\Fresh AIR Pack 1\10 Poly Synths Strange\01 5th Comper has now only 2 bytes diff to mem preset
        
        public override bool IncludePatchNameAtEnd { get; } = false;

        public override void PostProcess()
        {
            using (var ms = new MemoryStream(WzooBlock.BlockData))
            {
                ms.Seek(2048 + 76, SeekOrigin.Begin);
                ms.Write(PatchName,0,PatchName.Length);
                ms.WriteByte(0);

                WzooBlock.BlockData = ms.ToByteArray();
            }
            
            base.PostProcess();
        }

        public override byte[] GetEndChunk()
        {
            return VendorResources.VacuumProEndChunk;
        }
    }
}