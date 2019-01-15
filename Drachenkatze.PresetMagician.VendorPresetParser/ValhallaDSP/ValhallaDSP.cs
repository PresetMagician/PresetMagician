using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.ValhallaDSP
{
    public abstract class ValhallaDSP: AbstractVendorPresetParser
    {
        protected void DoScan(PresetBank rootBank, string directory)
        {
            var vc2parser = new VC2Parser(Plugin, "vpreset",Presets);
            vc2parser.DoScan(rootBank, directory);
        }
    }
}
