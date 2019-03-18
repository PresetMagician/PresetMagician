using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catel.Collections;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Interfaces
{
    public interface IVstService
    {
       
        void Save();
        byte[] GetPresetData(Preset preset);
        IRemotePluginInstance GetRemotePluginInstance(Plugin plugin, bool backgroundProcessing = true);
        Task<IRemotePluginInstance> GetInteractivePluginInstance(Plugin plugin);
        IRemoteVstService GetRemoteVstService();
        void Load();
        void SavePlugin(Plugin plugin);
    }
}