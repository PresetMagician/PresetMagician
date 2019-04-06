using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxBoom : Tfx
    {
        public override byte[] BlockMagic { get; } = {0x06, 0x16, 0x13, 0x30};

        public override bool IncludePatchNameAtEnd { get; } = true;

        public override byte[] GetEndChunk()
        {
            return VendorResources.BoomEndChunk;
        }
    }
}