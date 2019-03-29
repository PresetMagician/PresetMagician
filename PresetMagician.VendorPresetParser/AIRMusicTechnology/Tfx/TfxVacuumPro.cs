using System.IO;
using System.Linq;
using Catel.IO;
using GSF;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxVacuumPro : Tfx
    {
        public override byte[] BlockMagic { get; } = {0x05, 0xf7, 0x3c, 0xa8};

        public override void PostProcess()
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(LittleEndian.GetBytes(PatchName.Length + 1), 0, 4);
                ms.Write(PatchName, 0, PatchName.Length);
                ms.WriteByte(0);

                EndChunk = VendorResources.VacuumProEndChunk
                    .Concat(ms.ToByteArray()).ToArray();
            }
        }
    }
}