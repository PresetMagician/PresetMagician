using System.Threading.Tasks;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public class NullPresetParser : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override bool IsNullParser => true;

        public void ScanBanks()
        {
        }

#pragma warning disable 1998
        public override async Task DoScan()
#pragma warning restore 1998
        {
        }

        public override int GetNumPresets()
        {
            return 0;
        }

        public override bool CanHandle()
        {
            return true;
        }
    }
}