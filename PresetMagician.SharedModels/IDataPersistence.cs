using System.Threading.Tasks;
using SharedModels.Models;

namespace SharedModels
{
    public interface IDataPersistence
    {
        Task PersistPreset(Preset preset, byte[] data);
        Task Flush();
        Type GetOrCreateType(string typeName, string subTypeName = "");
        Mode GetOrCreateMode(string modeName);
    }
}