using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxMiniGrand : Tfx
    {
        public override byte[] BlockMagic { get; } = {0x06, 0x15, 0x99, 0xff};
        public override bool IncludeMidi { get; } = true;
        public override bool IncludePatchNameAtEnd { get; } = true;
        
        public override byte[] GetMidiData()
        {
            return VendorResources.MiniGrandDefaultMidi;
        }

        public override string GetMagicBlockPluginName()
        {
            return "Mini Grand";
        }
        
        public override byte[] GetEndChunk()
        {
            return VendorResources.MiniGrandEndChunk;
        }
    }
}