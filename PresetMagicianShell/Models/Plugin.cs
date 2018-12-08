using Catel.Data;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace PresetMagicianShell.Models
{
    public class Plugin : ModelBase
    {
        public VSTPlugin VstPlugin { get; set; }
        public IVendorPresetParser VstPresetParser { get; set; }

        public bool IsSupported
        {
            get
            {
                if (VstPresetParser == null)
                {
                    return false;

                }
                return !VstPresetParser.IsNullParser;
            }

            set
            {

            }

        }

       
    }
}