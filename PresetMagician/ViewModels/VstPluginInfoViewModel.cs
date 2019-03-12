using Catel.MVVM;
using PresetMagician.Core.Interfaces;
using SharedModels.Models;

namespace PresetMagician.ViewModels
{
    class VstPluginInfoViewModel : ViewModelBase
    {
        public VstPluginInfoViewModel(Plugin plugin)
        {
            Plugin = plugin;
            Title = "Plugin Info for " + Plugin.PluginName;
        }

        public Plugin Plugin { get; protected set; }
    }
}