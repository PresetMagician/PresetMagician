using System;
using Catel.Collections;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagicianShell.Models;

namespace PresetMagicianShell.Services.Interfaces
{
    public interface IVstService
    {
        event EventHandler SelectedPluginChanged;
        Plugin SelectedPlugin { get; set; }
        FastObservableCollection<Plugin> SelectedPlugins { get; }
        FastObservableCollection<Plugin> Plugins { get; }
        VstHost VstHost { get; set; }
    }
}