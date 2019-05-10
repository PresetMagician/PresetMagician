using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PresetMagician.Core.Enums;
using PresetMagician.Core.Models;
using PresetMagician.Utils.Logger;

namespace PresetMagician.Core.Interfaces
{
    public interface IVendorPresetParser
    {
        /// <summary>
        /// The priority of a given preset parser. This is used to determinate which plugin location
        /// is effectively used.
        /// </summary>
        PresetParserPriorityEnum Priority { get; }
        PresetBank RootBank { get; set; }
        IRemotePluginInstance PluginInstance { get; set; }
        int AudioPreviewPreDelay { get; set; }

        string Remarks { get; set; }
        string PresetParserType { get; }

        string BankLoadingNotes { get; set; }
        bool IsNullParser { get; }
        bool IsGenericParser { get; }
        PresetParserConfiguration PresetParserConfiguration { get; set; }

        int GetNumPresets();

        bool CanHandle();
        void Init();

        ObservableCollection<ObservableCollection<string>> DefaultTypes { get; set; }
        ObservableCollection<string> DefaultModes { get; set; }

        IDataPersistence DataPersistence { get; set; }

        List<int> GetSupportedPlugins();
        MiniLogger Logger { get; }

        Task DoScan();
        void OnAfterPresetExport();
    }
}