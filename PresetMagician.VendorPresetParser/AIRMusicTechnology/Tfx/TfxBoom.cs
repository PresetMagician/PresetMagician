using System.IO;
using System.Linq;
using Catel.IO;
using GSF;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxBoom : Tfx
    {
        public override byte[] BlockMagic { get; }=  {0x06, 0x16, 0x13, 0x30};

        public override void PostProcess()
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(LittleEndian.GetBytes(PatchName.Length+1), 0, 4);
                ms.Write(PatchName, 0, PatchName.Length);
                ms.WriteByte(0);

                EndChunk = VendorResources.BoomEndChunk
                    .Concat(ms.ToByteArray()).ToArray();
            }
        }
    }

}