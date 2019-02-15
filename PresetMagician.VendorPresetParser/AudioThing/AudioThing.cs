using Drachenkatze.PresetMagician.VendorPresetParser.Common;

namespace Drachenkatze.PresetMagician.VendorPresetParser.AudioThing
{
    public abstract class AudioThing : RecursiveVC2Parser
    {
        protected override string Extension { get; } = "atp";
    }
}