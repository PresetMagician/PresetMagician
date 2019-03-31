using System.IO;
using Catel.Collections;
using GSF;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxVelvet : Tfx
    {
        private static readonly int EXPECTED_NUM_PARAMETERS = 109;
        public override byte[] BlockMagic { get; } = {0x00, 0x00, 0x1f, 0xd6};

        public override void PostProcess()
        {
            if (Parameters.Count > EXPECTED_NUM_PARAMETERS)
            {
                var excessParameters = Parameters.Count - EXPECTED_NUM_PARAMETERS;

                for (var i = 0; i < excessParameters; i++)
                {
                    Parameters.RemoveLast();
                }
            }

            using (var subBlock = new MemoryStream())
            {
                subBlock.WriteByte(0xA8);
                subBlock.WriteByte(0x1D);
                subBlock.WriteByte(0xED);
                subBlock.WriteByte(0x0B);

                for (var i = 0; i < PatchName.Length; i++)
                {
                    subBlock.WriteByte(PatchName[i]);
                }

                for (var i = 0; i < 1024 - PatchName.Length; i++)
                {
                    subBlock.WriteByte(0x00);
                }

                using (var msbuf3 = new MemoryStream())
                {
                    msbuf3.WriteByte(0x00);
                    msbuf3.WriteByte(0x00);
                    msbuf3.WriteByte(0x00);
                    msbuf3.WriteByte(0x00);

                    var subBlockData = subBlock.ToArray();
                    var subBlockLength = BigEndian.GetBytes(subBlockData.Length);

                    msbuf3.Write(subBlockLength, 0, subBlockLength.Length);
                    msbuf3.Write(subBlockData, 0, subBlockData.Length);


                    msbuf3.Write(DEADBEEF, 0, DEADBEEF.Length);

                    WzooBlock.BlockData = msbuf3.ToArray();
                }
            }

        }
        
        public override byte[] GetEndChunk()
        {
            return VendorResources.VelvetEndChunk;
        }
    }
}