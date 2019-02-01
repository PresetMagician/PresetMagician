using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Catel.Collections;
using SharedModels;

namespace PresetMagician.Services.Interfaces
{
    public interface IVstService
    {
        event EventHandler SelectedPluginChanged;
        Plugin SelectedPlugin { get; set; }
        FastObservableCollection<Plugin> SelectedPlugins { get; }
        ObservableCollection<Plugin> Plugins { get; set; }

        FastObservableCollection<Preset> PresetExportList { get; }
        Preset SelectedExportPreset { get; set; }
        FastObservableCollection<Preset> SelectedPresets { get; }
        event EventHandler SelectedExportPresetChanged;
        Task SavePlugins();
        byte[] GetPresetData(Preset preset);
        IRemotePluginInstance GetRemotePluginInstance(Plugin plugin);
        Task<IRemotePluginInstance> GetInteractivePluginInstance(Plugin plugin);
        IRemoteVstService GetVstService();
        List<PluginLocation> GetPluginLocations(Plugin plugin);
    }
}