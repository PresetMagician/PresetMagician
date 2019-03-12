using System;
using System.Threading.Tasks;
using PresetMagician.Core.EventArgs;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Interfaces
{
    public interface IDataPersistence
    {
        event EventHandler<PresetUpdatedEventArgs> PresetUpdated;
        Task PersistPreset(Preset preset, byte[] data);
    }
}