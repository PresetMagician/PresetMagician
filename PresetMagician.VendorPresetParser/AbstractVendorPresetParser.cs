using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Catel.IoC;
using PresetMagician.Core.Enums;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Utils.Logger;

namespace PresetMagician.VendorPresetParser
{
    public abstract partial class AbstractVendorPresetParser
    {
        protected const string BankNameFactory = "Factory";
        protected const string BankNameUser = "User";

        private IRemotePluginInstance _pluginInstance;

        protected AbstractVendorPresetParser()
        {
            PresetParserConfiguration = new PresetParserConfiguration();
        }

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

        public virtual int AudioPreviewPreDelay { get; set; } = 40;
        
        public virtual PresetParserPriorityEnum Priority { get; } = PresetParserPriorityEnum.DEFAULT_PRIORITY;

        public virtual string Remarks { get; set; }

        public virtual bool IsNullParser => false;
        public virtual bool IsGenericParser => false;

        public string PresetParserType => GetType().Name;

        public PresetBank RootBank { get; set; } = new PresetBank();

        public ObservableCollection<ObservableCollection<string>> DefaultTypes { get; set; } =
            new ObservableCollection<ObservableCollection<string>>();

        public ObservableCollection<string> DefaultModes { get; set; } = new ObservableCollection<string>();

        public virtual string BankLoadingNotes { get; set; }
        public IDataPersistence DataPersistence { get; set; }
        public PresetParserConfiguration PresetParserConfiguration { get; set; }
        public MiniLogger Logger { get; } = new MiniMemoryLogger();

        public List<int> GetSupportedPlugins()
        {
            return SupportedPlugins;
        }


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