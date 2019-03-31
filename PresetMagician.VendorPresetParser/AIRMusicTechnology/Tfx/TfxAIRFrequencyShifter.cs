using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxAIRFrequencyShifter : Tfx
    {
        public override byte[] BlockMagic { get; } = {0x2f,0x44,0x65,0x66,};
        public override bool IncludeMidi { get; } = true;
        public override bool IncludePatchNameAtEnd { get; } = true;

       public override string GetMagicBlockPluginName()
       {
           return "";
       }
        
        public override byte[] GetMidiData()
        {
            return new byte[0];
        }
        
        public override byte[] GetEndChunk()
        {
            return VendorResources.AirFxSuiteEndChunk;
        }
    }
}