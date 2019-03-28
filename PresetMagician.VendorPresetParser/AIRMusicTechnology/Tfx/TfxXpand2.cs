using System.IO;
using System.Linq;
using Catel.IO;
using Drachenkatze.PresetMagician.VendorPresetParser.Properties;
using GSF;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxXpand2 : Tfx
    {
        public override byte[] WzooPluginId { get; } = {0x57, 0x53, 0x4C, 0x31};
        public override byte[] BlockMagic { get; } = {0x0C, 0x0B, 0xA2, 0x28};

        public override void PostProcess()
        {
            // Fix for some tfx files not containing all required parameters
            if (Parameters.Count == 114)
            {
                // fill missing parameters for all parts
                Parameters.Add(0);             // Part A Voice Mode
                Parameters.Add(0);             // Part A Mono Mode
                Parameters.Add(0.49);          // Part A Polyphony
                Parameters.Add(0.16666);       // Part A PB Range
                Parameters.Add(0.5);           // Part B Attack
                Parameters.Add(0.5);           // Part B Decay
                Parameters.Add(0.5);           // Part B Release
                Parameters.Add(0.5);           // Part B Cutoff
                Parameters.Add(0.5);           // Part B Env Depth
                Parameters.Add(0.5);           // Part B Fine Tune
                Parameters.Add(0);             // Part B Voice Mode
                Parameters.Add(0);             // Part B Mono Mode
                Parameters.Add(0.49);          // Part B Polyphony
                Parameters.Add(0.16666);       // Part B PB Range
                Parameters.Add(0.5);           // Part C Attack
                Parameters.Add(0.5);           // Part C Decay
                Parameters.Add(0.5);           // Part C Release
                Parameters.Add(0.5);           // Part C Cutoff
                Parameters.Add(0.5);           // Part C Env Depth
                Parameters.Add(0.5);           // Part C Fine Tune
                Parameters.Add(0);             // Part C Voice Mode
                Parameters.Add(0);             // Part C Mono Mode
                Parameters.Add(0.49);          // Part C Polyphony
                Parameters.Add(0.16666);       // Part C PB Range
                Parameters.Add(0.5);           // Part D Attack
                Parameters.Add(0.5);           // Part D Decay
                Parameters.Add(0.5);           // Part D Release
                Parameters.Add(0.5);           // Part D Cutoff
                Parameters.Add(0.5);           // Part D Env Depth
                Parameters.Add(0.5);           // Part D Fine Tune
                Parameters.Add(0);             // Part D Voice Mode
                Parameters.Add(0);             // Part D Mono Mode
                Parameters.Add(0.49);          // Part D Polyphony
                Parameters.Add(0.16666);       // Part D PB Range
                
            }

            using (var ms = new MemoryStream())
            {
                ms.Write(LittleEndian.GetBytes(PatchName.Length), 0, 4);
                ms.Write(PatchName, 0, PatchName.Length);
                ms.WriteByte(0);

                EndChunk = Resource1.Xpand2EndChunk
                    .Concat(ms.ToByteArray()).ToArray();
            }
        }

        public override byte[] GetBlockDataToWrite()
        {
            if (WzooBlock.IsMagicBlock)
            {
                WzooBlock.PluginName = WzooBlock.PluginName.Concat(new byte[] {0x00, 0x32}).ToArray();
            }


            var data = WzooBlock.GetDataToWrite();


            return data;
        }
    }
}