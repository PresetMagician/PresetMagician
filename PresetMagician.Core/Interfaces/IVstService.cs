using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catel.Collections;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Interfaces
{
    public interface IVstService
    {
        event EventHandler SelectedPluginChanged;
        Plugin SelectedPlugin { get; set; }
        FastObservableCollection<Plugin> SelectedPlugins { get; }
        FastObservableCollection<Plugin> Plugins { get; set; }

        FastObservableCollection<Preset> PresetExportList { get; }
        Preset SelectedExportPreset { get; set; }
        FastObservableCollection<Preset> SelectedPresets { get; }
        event EventHandler SelectedExportPresetChanged;
        void Save();
        byte[] GetPresetData(Preset preset);
        IRemotePluginInstance GetRemotePluginInstance(Plugin plugin, bool backgroundProcessing = true);
        Task<IRemotePluginInstance> GetInteractivePluginInstance(Plugin plugin);
        IRemoteVstService GetRemoteVstService();
        void Load();
        void SavePlugin(Plugin plugin);
    }
}