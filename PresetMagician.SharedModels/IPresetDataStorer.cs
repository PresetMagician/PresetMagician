using System.Threading.Tasks;

namespace SharedModels
{
    public interface IPresetDataStorer
    {
        Task PersistPreset(Preset preset, byte[] data);
        Task Flush();
    }
}