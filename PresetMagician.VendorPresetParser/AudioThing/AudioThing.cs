using PresetMagician.VendorPresetParser.Common;

namespace PresetMagician.VendorPresetParser.AudioThing
{
    public abstract class AudioThing : RecursiveVC2Parser
    {
        protected override string Extension { get; } = "atp";
    }
}