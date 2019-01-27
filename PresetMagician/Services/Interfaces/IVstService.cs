using System;
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
        FastObservableCollection<Plugin> CachedPlugins { get; }
        event EventHandler SelectedExportPresetChanged;
        Task SavePlugins();
        byte[] GetPresetData(Preset preset);
        Task<IRemotePluginInstance> GetRemotePluginInstance(Plugin plugin);
        IRemotePluginInstance GetInteractivePluginInstance(Plugin plugin);
    }
}