using PresetMagician.Core.Models;

namespace PresetMagician.Core.Interfaces
{
    public interface IVstService
    {
        void Save();

        void Load();
        void SavePlugin(Plugin plugin);
    }
}