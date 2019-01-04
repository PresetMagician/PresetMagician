namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public class NullPresetParser : AbstractVendorPresetParser, IVendorPresetParser
    {
        public override bool IsNullParser => true;

        public void ScanBanks()
        {
        }

        public override bool CanHandle()
        {
            return true;
        }
    }
}