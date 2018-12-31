using System;
using Catel.Collections;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagician.Models;

namespace PresetMagician.Services.Interfaces
{
    public interface IVstService
    {
        event EventHandler SelectedPluginChanged;
        Plugin SelectedPlugin { get; set; }
        FastObservableCollection<Plugin> SelectedPlugins { get; }
        FastObservableCollection<Plugin> Plugins { get; }
        VstHost VstHost { get; set; }
        FastObservableCollection<Preset> PresetExportList { get; }
        Preset SelectedExportPreset { get; set; }
        FastObservableCollection<Preset> SelectedPresets { get; }
        FastObservableCollection<Plugin> CachedPlugins { get; }
        event EventHandler SelectedExportPresetChanged;
    }
}