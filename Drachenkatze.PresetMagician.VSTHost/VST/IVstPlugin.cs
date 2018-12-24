using System;
using Drachenkatze.PresetMagician.VSTHost.VST.EventArgs;
using Jacobi.Vst.Interop.Host;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public interface IVstPlugin
    {
        string DllPath { get; }
        VstPluginContext PluginContext { get; set; }
        void OnLoaded();
        void OnLoadError(Exception e);
        int PluginId { get; set; }
        string PluginName { get; }
        VstHost.PluginTypes PluginType { get; set; } 
        int NumPresets { get; }
        string PluginVendor { get; set; }
    }
}