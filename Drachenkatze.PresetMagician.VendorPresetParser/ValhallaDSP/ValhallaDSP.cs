using Drachenkatze.PresetMagician.VendorPresetParser.Common;

namespace Drachenkatze.PresetMagician.VendorPresetParser.ValhallaDSP
{
    public abstract class ValhallaDSP : RecursiveVC2Parser
    {
        protected override string Extension { get; } = "vpreset";
    }
}