

using PresetMagician.Core.Interfaces;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public class NullPresetParser : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override bool IsNullParser => true;

        public override bool CanHandle()
        {
            return true;
        }
    }
}