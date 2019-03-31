using PresetMagician.VendorPresetParser.Properties;

namespace PresetMagician.VendorPresetParser.AIRMusicTechnology.Tfx
{
    public class TfxDb33Fx : Tfx
    {
        public override byte[] BlockMagic { get; } = {0x06, 0x15, 0xc1, 0x28};
        public override bool IncludeMidi { get; } = true;
        public override bool IncludePatchNameAtEnd { get; } = true;

        public override void PostProcess()
        {
            for (var i = 0; i < 18; i++)
            {
                Parameters.RemoveAt(0);
            }
        }

        public override byte[] GetMidiData()
        {
            return VendorResources.Db33FxDefaultMidi;
        }
        
        public override string GetMagicBlockPluginName()
        {
            return "DB-33FX";
        }
        
        public override byte[] GetEndChunk()
        {
            return VendorResources.Db33FxEndChunk;
        }
    }
}