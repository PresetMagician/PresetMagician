using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxDb33 : Tfx
    {
        public override byte[] BlockMagic { get; } = {0x06, 0x15, 0xc1, 0x28};
        public override bool IncludeMidi { get; } = true;
        public override bool IncludePatchNameAtEnd { get; } = true;
        
        public override void PostProcess()
        {
            if (Parameters.Count == 32)
            {
                Parameters.RemoveAt(19);
                Parameters[19] = Parameters[1];
            }
        }

        public override byte[] GetMidiData()
        {
            return VendorResources.Db33DefaultMidi;
        }

        public override string GetMagicBlockPluginName()
        {
            return "DB-33";
        }
        
        public override byte[] GetEndChunk()
        {
            return VendorResources.Db33EndChunk;
        }
    }
}