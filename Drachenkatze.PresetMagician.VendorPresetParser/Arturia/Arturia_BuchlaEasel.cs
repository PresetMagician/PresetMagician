using System.Collections.Generic;

namespace Drachenkatze.PresetMagician.VendorPresetParser.Arturia
{
    public class Arturia_BuchlaEasel: Arturia, IVendorPresetParser
    {
        public override List<int> SupportedPlugins => new List<int> { 1114981729 };

        public void ScanBanks()
        {
            List<string> instruments = new List<string>();
            instruments.Add("Buchla Easel");
            ScanPresets(instruments);
        }
    }
}