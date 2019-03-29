using System.IO;
using System.Linq;
using Catel.IO;
using GSF;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxDb33 : Tfx
    {
        public override byte[] BlockMagic { get; } = {0x06, 0x15, 0xc1, 0x28};

        public override void PostProcess()
        {
            if (Parameters.Count == 32)
            {
                Parameters.RemoveAt(19);
                Parameters[19] = Parameters[1];
            }

            using (var ms = new MemoryStream())
            {
                ms.Write(LittleEndian.GetBytes(PatchName.Length + 1), 0, 4);
                ms.Write(PatchName, 0, PatchName.Length);
                ms.WriteByte(0);

                EndChunk = VendorResources.Db33EndChunk
                    .Concat(ms.ToByteArray()).ToArray();
            }

            ParseMidi();
        }

        public override byte[] GetBlockDataToWrite()
        {
            if (!MidiBlock.IsMagicBlock)
            {
                MidiBlock.PluginName = new byte[]
                    {0x00, 0x44, 0x00, 0x42, 0x00, 0x2d, 0x00, 0x33, 0x00, 0x33};
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
                    ms.Write(BigEndian.GetBytes(VendorResources.Db33DefaultMidi.Length), 0, 4);
                    ms.Write(VendorResources.Db33DefaultMidi, 0, VendorResources.Db33DefaultMidi.Length);

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