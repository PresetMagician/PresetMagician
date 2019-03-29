using System.IO;
using System.Linq;
using Catel.IO;
using GSF;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxMiniGrand : Tfx
    {
        public override byte[] BlockMagic { get; } = {0x06, 0x15, 0x99, 0xff};

        public override void PostProcess()
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(LittleEndian.GetBytes(PatchName.Length + 1), 0, 4);
                ms.Write(PatchName, 0, PatchName.Length);
                ms.WriteByte(0);

                EndChunk = VendorResources.MiniGrandEndChunk
                    .Concat(ms.ToByteArray()).ToArray();
            }

            ParseMidi();
        }

        public override byte[] GetBlockDataToWrite()
        {
            if (!MidiBlock.IsMagicBlock)
            {
                MidiBlock.PluginName = new byte[]
                {
                    0x00, 0x4d, 0x00, 0x69, 0x00, 0x6e, 0x00, 0x69, 0x00, 0x20, 0x00, 0x47, 0x00, 0x72, 0x00, 0x61,
                    0x00, 0x6e, 0x00, 0x64
                };
                MidiBlock.IsMagicBlock = true;

                using (var ms = new MemoryStream())
                {
                    // add 4 null bytes
                    ms.WriteByte(0x00);
                    ms.WriteByte(0x00);
                    ms.WriteByte(0x00);
                    ms.WriteByte(0x00);

                    // add 4 FF bytes
                    ms.WriteByte(0xFF);
                    ms.WriteByte(0xFF);
                    ms.WriteByte(0xFF);
                    ms.WriteByte(0xFF);

                    //add block length
                    ms.Write(BigEndian.GetBytes(VendorResources.MiniGrandDefaultMidi.Length), 0, 4);
                    ms.Write(VendorResources.MiniGrandDefaultMidi, 0, VendorResources.MiniGrandDefaultMidi.Length);

                    //add deadbeef
                    ms.WriteByte(0xDE);
                    ms.WriteByte(0xAD);
                    ms.WriteByte(0xBE);
                    ms.WriteByte(0xEF);

                    MidiBlock.BlockData = ms.ToByteArray();
                }
            }


            var data = WzooBlock.GetDataToWrite().Concat(MidiBlock.GetDataToWrite()).ToArray();
            return data;
        }
    }
}