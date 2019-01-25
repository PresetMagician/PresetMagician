using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Jacobi.Vst.Core;
using PresetMagician.Models;
using PresetMagician.SharedModels;
using SharedModels;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public abstract class AbstractVendorPresetParser
    {
        protected const string BankNameFactory = "Factory";
        protected const string BankNameUser = "User";

        public virtual bool SupportsAdditionalBankFiles { get; set; } = false;
        public virtual List<BankFile> AdditionalBankFiles { get; } = null;

        public Plugin Plugin { get; set; }
        public virtual List<int> SupportedPlugins => new List<int>();

        public List<int> GetSupportedPlugins()
        {
            return SupportedPlugins;
        }

        public virtual int AudioPreviewPreDelay { get; set; } = 40;

        public virtual string Remarks { get; set; }

        public virtual bool IsNullParser => false;

        public string PresetParserType => GetType().Name;

        public PresetBank RootBank { get; set; } = new PresetBank();

        public ObservableCollection<ObservableCollection<string>> DefaultTypes { get; set; } =
            new ObservableCollection<ObservableCollection<string>>();

        public ObservableCollection<string> DefaultModes { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<Preset> Presets { get; set; } = new ObservableCollection<Preset>();
        public IRemoteVstService RemoteVstService { get; set; }

        public virtual string BankLoadingNotes { get; set; }
        public IPresetDataStorer PresetDataStorer { get; set; }

        public virtual int GetNumPresets()
        {
            return 99999;
            throw new NotImplementedException();
        }

        public virtual void Init()
        {
        }

        public virtual async Task DoScan()
        {
            //throw new NotImplementedException();
        }

        public void ScanBanks()
        {
            //throw new NotImplementedException();
        }

        public virtual bool CanHandle()
        {
            if (SupportedPlugins.Contains(Plugin.PluginId))
            {
                return true;
            }

            return false;
        }

        public virtual void OnAfterPresetExport()
        {
        }
    }
}