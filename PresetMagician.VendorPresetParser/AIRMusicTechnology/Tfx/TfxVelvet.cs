using Drachenkatze.PresetMagician.VendorPresetParser.Properties;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxVelvet : Tfx
    {
        public override byte[] BlockMagic { get; }=  {0x06, 0x16, 0x13, 0x30};

        public override void PostProcess()
        {


            EndChunk = Resource1.VacuumEndChunk;
        }
    }

}