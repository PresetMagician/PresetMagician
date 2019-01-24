using System.Threading.Tasks;
using SharedModels;

namespace PresetMagician.Services.Interfaces
{
    public interface INativeInstrumentsResourceGeneratorService
    {
        void AutoGenerateResources(Plugin plugin, IRemoteVstService remoteVstService);
        void GenerateResources(Plugin plugin, IRemoteVstService remoteVstService, bool force = false);
        bool ShouldCreateScreenshot(Plugin plugin);
    }
}