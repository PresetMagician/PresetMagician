using PresetMagician.Core.Models;

namespace PresetMagician.Core.Interfaces
{
    public interface IVstService
    {
        void Save();
        byte[] GetPresetData(Preset preset);

        void Load();
        void SavePlugin(Plugin plugin);
    }
}