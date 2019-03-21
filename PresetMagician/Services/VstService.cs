using System.Collections.Generic;
using System.Threading.Tasks;
using Catel;
using Catel.Collections;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.RemoteVstHost;
using PresetMagician.RemoteVstHost.Processes;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.Services
{
    public class VstService : IVstService
    {
        private readonly IApplicationService _applicationService;
        private VstHostProcess _interactiveVstHostProcess;
        private readonly PresetDataPersisterService _presetDataPersisterService;
        private readonly DataPersisterService _dataPersister;
        private readonly GlobalService _globalService;

        private readonly Dictionary<Plugin, IRemotePluginInstance> _pluginInstances =
            new Dictionary<Plugin, IRemotePluginInstance>();

        public VstService(IApplicationService applicationService, GlobalService globalService,
            DataPersisterService dataPersister, PresetDataPersisterService presetDataPersisterService)
        {
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => dataPersister);
            Argument.IsNotNull(() => presetDataPersisterService);

            _applicationService = applicationService;
            _presetDataPersisterService = presetDataPersisterService;
            _dataPersister = dataPersister;
            _globalService = globalService;

            Plugins = _globalService.Plugins;
        }

        public void Save()
        {
            _dataPersister.Save();
        }

        public void SavePlugin(Plugin plugin)
        {
            _dataPersister.SavePlugin(plugin);
        }

        public void Load()
        {
            _dataPersister.Load();
        }

        public IRemoteVstService GetRemoteVstService()
        {
            return _globalService.RemoteVstHostProcessPool.GetVstService();
        }


        public byte[] GetPresetData(Preset preset)
        {
            return _presetDataPersisterService.GetPresetData(preset);
        }

        public async Task<IRemotePluginInstance> GetInteractivePluginInstance(Plugin plugin)
        {
            if (_interactiveVstHostProcess == null)
            {
                _interactiveVstHostProcess = new VstHostProcess(20, true);
                _interactiveVstHostProcess.Start();
                _interactiveVstHostProcess.WaitUntilStarted();
            }

            if (!_pluginInstances.ContainsKey(plugin))
            {
                _pluginInstances.Add(plugin, new RemotePluginInstance(_interactiveVstHostProcess, plugin, true, true));
            }

            return _pluginInstances[plugin];
        }

        public FastObservableCollection<Plugin> Plugins { get; set; } = new FastObservableCollection<Plugin>();
    }
}