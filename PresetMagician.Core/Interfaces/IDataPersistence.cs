using System.Threading.Tasks;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Interfaces
{
    public interface IDataPersistence
    {
        Task PersistPreset(Preset preset, byte[] data);
    }
}