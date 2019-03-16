using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catel;
using Catel.Collections;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.RemoteVstHost;
using PresetMagician.RemoteVstHost.Processes;
using PresetMagician.Services.Interfaces;
using PresetMagician.Core.Interfaces;


namespace PresetMagician.Services
{
    public class VstService : IVstService
    {
        private readonly IApplicationService _applicationService;
        private VstHostProcess _interactiveVstHostProcess;
        private readonly PresetDataPersisterService _presetDataPersisterService;
        private readonly DataPersisterService _dataPersister;
        private readonly Dictionary<Plugin, IRemotePluginInstance> _pluginInstances =
            new Dictionary<Plugin, IRemotePluginInstance>();

        public VstService(IApplicationService applicationService, GlobalService globalService, DataPersisterService dataPersister, PresetDataPersisterService presetDataPersisterService)
        {
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => dataPersister);
            Argument.IsNotNull(() => presetDataPersisterService);

            _applicationService = applicationService;
            _presetDataPersisterService = presetDataPersisterService;
            _dataPersister = dataPersister;


            Plugins = globalService.Plugins;
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
            return _applicationService.NewProcessPool.GetVstService();
        }
        
        

        public byte[] GetPresetData(Preset preset)
        {
            return _presetDataPersisterService.GetPresetData(preset);
        }

        public IRemotePluginInstance GetRemotePluginInstance(Plugin plugin, bool backgroundProcessing = true)
        {
            return _applicationService.NewProcessPool.GetRemotePluginInstance(plugin, backgroundProcessing);
        }

       
        public  async Task<IRemotePluginInstance> GetInteractivePluginInstance(Plugin plugin)
        {
            if (_interactiveVstHostProcess == null)
            {
                _interactiveVstHostProcess = new VstHostProcess(20, true);
                _interactiveVstHostProcess.Start();
                await _interactiveVstHostProcess.WaitUntilStarted();
            }
            
            if (!_pluginInstances.ContainsKey(plugin))
            {

                _pluginInstances.Add(plugin, new RemotePluginInstance(_interactiveVstHostProcess, plugin, true, true));
            }

            return _pluginInstances[plugin];
        }


        public FastObservableCollection<Plugin> SelectedPlugins { get; } = new FastObservableCollection<Plugin>();
        public FastObservableCollection<Plugin> Plugins { get; set; } = new FastObservableCollection<Plugin>();

        public FastObservableCollection<Preset> SelectedPresets { get; } = new FastObservableCollection<Preset>();
        public FastObservableCollection<Preset> PresetExportList { get; } = new FastObservableCollection<Preset>();

        #region SelectedPlugin

        private Plugin _selectedPlugin;

        public Plugin SelectedPlugin
        {
            get => _selectedPlugin;
            set
            {
                _selectedPlugin = value;
                SelectedPluginChanged?.Invoke(this, System.EventArgs.Empty);
            }
        }

        public event EventHandler SelectedPluginChanged;

        #endregion

        #region SelectedExportPreset

        private Preset _selectedExportPreset;

        public Preset SelectedExportPreset
        {
            get => _selectedExportPreset;
            set
            {
                _selectedExportPreset = value;
                SelectedExportPresetChanged?.Invoke(this, System.EventArgs.Empty);
            }
        }

        public event EventHandler SelectedExportPresetChanged;

        #endregion
    }
}