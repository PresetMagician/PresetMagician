using System.IO;
using System.Text;
using Catel.IO;
using GSF;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxXpand2 : Tfx
    {
        public override byte[] BlockMagic { get; } = {0x0C, 0x0B, 0xA2, 0x28};
        public override bool IncludeMidi { get; } = true;
        public override bool IncludePatchNameAtEnd { get; } = true;
        
        public override void PostProcess()
        {
            // Fix for some tfx files not containing all required parameters
            if (Parameters.Count == 114)
            {
                // fill missing parameters for all parts
                Parameters.Add(0); // Part A Voice Mode
                Parameters.Add(0); // Part A Mono Mode
                Parameters.Add(0.49); // Part A Polyphony
                Parameters.Add(0.16666); // Part A PB Range
                Parameters.Add(0.5); // Part B Attack
                Parameters.Add(0.5); // Part B Decay
                Parameters.Add(0.5); // Part B Release
                Parameters.Add(0.5); // Part B Cutoff
                Parameters.Add(0.5); // Part B Env Depth
                Parameters.Add(0.5); // Part B Fine Tune
                Parameters.Add(0); // Part B Voice Mode
                Parameters.Add(0); // Part B Mono Mode
                Parameters.Add(0.49); // Part B Polyphony
                Parameters.Add(0.16666); // Part B PB Range
                Parameters.Add(0.5); // Part C Attack
                Parameters.Add(0.5); // Part C Decay
                Parameters.Add(0.5); // Part C Release
                Parameters.Add(0.5); // Part C Cutoff
                Parameters.Add(0.5); // Part C Env Depth
                Parameters.Add(0.5); // Part C Fine Tune
                Parameters.Add(0); // Part C Voice Mode
                Parameters.Add(0); // Part C Mono Mode
                Parameters.Add(0.49); // Part C Polyphony
                Parameters.Add(0.16666); // Part C PB Range
                Parameters.Add(0.5); // Part D Attack
                Parameters.Add(0.5); // Part D Decay
                Parameters.Add(0.5); // Part D Release
                Parameters.Add(0.5); // Part D Cutoff
                Parameters.Add(0.5); // Part D Env Depth
                Parameters.Add(0.5); // Part D Fine Tune
                Parameters.Add(0); // Part D Voice Mode
                Parameters.Add(0); // Part D Mono Mode
                Parameters.Add(0.49); // Part D Polyphony
                Parameters.Add(0.16666); // Part D PB Range
            }

            if (!WzooBlock.IsMagicBlock)
            {
                WzooBlock.PluginName = Encoding.BigEndianUnicode.GetBytes(GetMagicBlockPluginName());
                WzooBlock.IsMagicBlock = true;

                var oldData = WzooBlock.BlockData;

                using (var ms = new MemoryStream())
                {
                    // add 4 null bytes
                    ms.WriteByte(0x00);
                    ms.WriteByte(0x00);
                    ms.WriteByte(0x00);
                    ms.WriteByte(0x00);

                    //add block length
                    ms.Write(BigEndian.GetBytes(oldData.Length), 0, 4);
                    ms.Write(oldData, 0, oldData.Length);

                    //add deadbeef
                    ms.Write(DEADBEEF, 0, DEADBEEF.Length);

                    WzooBlock.BlockData = ms.ToByteArray();
                }
            }
            else
            {
                WzooBlock.PluginName = Encoding.BigEndianUnicode.GetBytes(GetMagicBlockPluginName());
            }
        }

        public override byte[] GetMidiData()
        {
            return VendorResources.Xpand2DefaultMidi;
        }

        public override string GetMagicBlockPluginName()
        {
            return "Xpand!2";
        }
        
        public override byte[] GetEndChunk()
        {
            return VendorResources.Xpand2EndChunk;
        }
    }
}