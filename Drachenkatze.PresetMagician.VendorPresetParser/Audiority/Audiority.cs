using Drachenkatze.PresetMagician.VendorPresetParser.Common;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Audiority
{
    public abstract class Audiority: AbstractVendorPresetParser
    {
        protected void DoScan(PresetBank rootBank, string directory)
        {
            var vc2parser = new VC2Parser(Plugin, "aup",Presets);
            vc2parser.DoScan(rootBank, directory);
        }
    }
}
