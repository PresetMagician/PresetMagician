using System.Threading.Tasks;
using SharedModels;

namespace PresetMagician.Services.Interfaces
{
    public interface INativeInstrumentsResourceGeneratorService
    {
        Task AutoGenerateResources(Plugin plugin);
        Task GenerateResources(Plugin plugin);
    }
}