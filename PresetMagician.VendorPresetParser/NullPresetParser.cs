using PresetMagician.Core.Interfaces;

namespace PresetMagician.VendorPresetParser
{
    public class NullPresetParser : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override bool RequiresRescanWithEachRelease { get; } = true;
        public override bool IsNullParser => true;

        public override bool CanHandle()
        {
            return true;
        }
    }
}