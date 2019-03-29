using Drachenkatze.PresetMagician.VendorPresetParser.Properties;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxTheRiser : Tfx
    {
        public override byte[] BlockMagic { get; }=  {0x05, 0xf7, 0x3c, 0xa8};

        public override void PostProcess()
        {


            EndChunk = Resource1.TheRiserEndChunk;
        }
    }

}