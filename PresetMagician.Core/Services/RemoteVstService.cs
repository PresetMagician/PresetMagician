using System.Threading.Tasks;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Services
{
    public class RemoteVstService
    {
        private readonly GlobalService _globalService;


        public RemoteVstService(GlobalService globalService)
        {
            _globalService = globalService;
        }

        public IRemotePluginInstance GetInteractivePluginInstance(Plugin plugin, bool backgroundProcessing = true)
        {
            return _globalService.RemoteVstHostProcessPool.GetRemoteInteractivePluginInstance(plugin, backgroundProcessing);
        }

        public IRemotePluginInstance GetRemotePluginInstance(Plugin plugin,
            bool backgroundProcessing = true)
        {
            return _globalService.RemoteVstHostProcessPool.GetRemotePluginInstance(plugin, backgroundProcessing);
        }

        public IRemoteVstService GetRemoteVstService()
        {
            return _globalService.RemoteVstHostProcessPool.GetVstService();
        }
    }
}