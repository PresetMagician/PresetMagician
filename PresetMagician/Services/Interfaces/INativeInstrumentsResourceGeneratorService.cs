using SharedModels;

namespace PresetMagician.Services.Interfaces
{
    public interface INativeInstrumentsResourceGeneratorService
    {
        void AutoGenerateResources(IRemotePluginInstance pluginInstance);
        void GenerateResources(IRemotePluginInstance pluginInstance, bool force = false);
        bool ShouldCreateScreenshot(IRemotePluginInstance pluginInstance);
    }
}