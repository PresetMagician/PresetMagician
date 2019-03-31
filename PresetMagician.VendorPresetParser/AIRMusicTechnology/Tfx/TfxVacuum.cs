using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxVacuum : Tfx
    {
        public override byte[] BlockMagic { get; } = {0x06, 0x16, 0x13, 0x30};
        public override bool IncludeMidi { get; } = true;
        public override bool IncludePatchNameAtEnd { get; } = true;
        
        public override byte[] GetMidiData()
        {
            return VendorResources.VacuumDefaultMidi;
        }

        public override string GetMagicBlockPluginName()
        {
            return "Vacuum";
        }
        
        public override byte[] GetEndChunk()
        {
            return VendorResources.VacuumEndChunk;
        }
    }
}