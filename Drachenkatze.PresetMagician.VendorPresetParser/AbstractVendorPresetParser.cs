using System.Collections.Generic;
using System.Collections.ObjectModel;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public abstract class AbstractVendorPresetParser
    {
        protected const string BankNameFactory = "Factory";
        
        public IVstPlugin VstPlugin { get; set; }
        public virtual List<int> SupportedPlugins => new List<int>();

        public virtual int AudioPreviewPreDelay { get; set; } = 40;

        public virtual string Remarks { get; set; }

        public virtual bool IsNullParser => false;

        public string PresetParserType => GetType().Name;

        private readonly PresetBank _rootBank = new PresetBank();

        public PresetBank RootBank { get; set; } = new PresetBank();
       
        public ObservableCollection<Preset> Presets { get; } = new ObservableCollection<Preset>();

        public virtual bool CanHandle()
        {
            if (SupportedPlugins.Contains(VstPlugin.PluginId))
            {
                return true;
            }

            return false;
        }

        public virtual void OnAfterPresetExport(VstHost host, IVstPlugin plugin)
        {

        }

       
    }
}