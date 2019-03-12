using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Catel.Linq;
using Catel.Logging;
using Drachenkatze.PresetMagician.Utils;
using Jacobi.Vst.Core;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;

namespace Drachenkatze.PresetMagician.VendorPresetParser
{
    public abstract partial class AbstractVendorPresetParser
    {
        protected const string BankNameFactory = "Factory";
        protected const string BankNameUser = "User";

        protected List<BankFile> AdditionalBankFiles
        {
            get
            {
                if (_pluginInstance != null)
                {
                    return _pluginInstance.Plugin.AdditionalBankFiles.ToList();
                }

                return new List<BankFile>();
            }
        } 

        private IRemotePluginInstance _pluginInstance;

        public IRemotePluginInstance PluginInstance
        {
            get => _pluginInstance;
            set
            {
                if (value == null)
                {
                    return;
                }

                if (_pluginInstance != value)
                {
                    PresetSaveMode = PresetSaveModes.NotYetDetermined;
                    PresetHashes.Clear();
                }
                
                _pluginInstance = value;
            }
        }

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

        public virtual string BankLoadingNotes { get; set; }
        public IDataPersistence DataPersistence { get; set; }

        public virtual int GetNumPresets()
        {
            return GetAdditionalBanksPresetCount();
        }

        public virtual void Init()
        {
        }

        public virtual async Task DoScan()
        {
            if (AdditionalBankFiles.Count > 0)
            {
                await PluginInstance.LoadPlugin();
                await ParseAdditionalBanks();    
            }
        }
        
        public virtual bool CanHandle()
        {
            return SupportedPlugins.Contains(PluginInstance.Plugin.VstPluginId);
        }

        public virtual void OnAfterPresetExport()
        {
        }
    }
}