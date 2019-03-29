using PresetMagician.VendorPresetParser.Common;

namespace PresetMagician.VendorPresetParser.ValhallaDSP
{
    public abstract class ValhallaDSP : RecursiveVC2Parser
    {
        protected override string Extension { get; } = "vpreset";
    }
}