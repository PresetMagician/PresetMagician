using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxTheRiser : Tfx
    {
        public override byte[] BlockMagic { get; }=  {0x05, 0xf7, 0x3c, 0xa8};

        public override void PostProcess()
        {


            EndChunk = VendorResources.TheRiserEndChunk;
        }
    }

}