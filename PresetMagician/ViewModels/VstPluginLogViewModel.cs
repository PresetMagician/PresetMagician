using Catel.MVVM;
using PresetMagician.Core.Models;

namespace PresetMagician.ViewModels
{
    class VstPluginLogViewModel : ViewModelBase
    {
        public VstPluginLogViewModel(Plugin plugin)
        {
            Plugin = plugin;
            Title = "Plugin Errors for " + Plugin.DllFilename;
        }

        public Plugin Plugin { get; protected set; }
    }
}