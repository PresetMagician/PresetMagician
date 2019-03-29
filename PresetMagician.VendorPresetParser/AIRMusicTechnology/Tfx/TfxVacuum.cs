using System.IO;
using System.Linq;
using Catel.IO;
using GSF;
using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxVacuum : Tfx
    {
        public override byte[] BlockMagic { get; }=  {0x06, 0x16, 0x13, 0x30};

        public override void PostProcess()
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(LittleEndian.GetBytes(PatchName.Length+1), 0, 4);
                ms.Write(PatchName, 0, PatchName.Length);
                ms.WriteByte(0);

                EndChunk = VendorResources.VacuumEndChunk
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
                    0x00, 0x56, 0x00, 0x61, 0x00, 0x63, 0x00, 0x75, 0x00, 0x75, 0x00, 0x6d
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
                    ms.Write(BigEndian.GetBytes(VendorResources.VacuumDefaultMidi.Length), 0, 4);
                    ms.Write(VendorResources.VacuumDefaultMidi, 0, VendorResources.VacuumDefaultMidi.Length);

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