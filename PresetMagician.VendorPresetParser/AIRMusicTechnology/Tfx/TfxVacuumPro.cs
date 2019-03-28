using Drachenkatze.PresetMagician.VendorPresetParser.Properties;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxVacuumPro : Tfx
    {
        public override byte[] WzooPluginId { get; }=  {0x56, 0x61, 0x63, 0x50};
        public override byte[] BlockMagic { get; }=  {0x05, 0xf7, 0x3c, 0xa8};

        public override void PostProcess()
        {


            EndChunk = Resource1.VacuumProEndChunk;
        }
    }

}