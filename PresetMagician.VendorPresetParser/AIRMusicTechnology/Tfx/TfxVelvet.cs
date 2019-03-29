using System.IO;
using Catel.Collections;
using GSF;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxVelvet : Tfx
    {
        public override byte[] BlockMagic { get; }=  {0x00, 0x00, 0x1f, 0xd6};

        private static readonly int EXPECTED_NUM_PARAMETERS = 109;
            
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



                    msbuf3.WriteByte(0xDE);
                    msbuf3.WriteByte(0xAD);
                    msbuf3.WriteByte(0xBE);
                    msbuf3.WriteByte(0xEF);

                    WzooBlock.BlockData = msbuf3.ToArray();
                }
            }



            EndChunk = VendorResources.VelvetEndChunk;
        }
    }

}