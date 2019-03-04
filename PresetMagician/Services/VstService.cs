using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catel;
using Catel.Collections;
using Catel.Threading;
using PresetMagician.RemoteVstHost;
using PresetMagician.RemoteVstHost.Processes;
using PresetMagician.Services.Interfaces;
using SharedModels;
using SharedModels.Collections;

namespace PresetMagician.Services
{
    public class VstService : IVstService
    {
        private readonly IDatabaseService _databaseService;
        private readonly IApplicationService _applicationService;
        private VstHostProcess _interactiveVstHostProcess;
        private readonly Dictionary<Plugin, IRemotePluginInstance> _pluginInstances =
            new Dictionary<Plugin, IRemotePluginInstance>();

        public VstService(IDatabaseService databaseService, IApplicationService applicationService)
        {
            Argument.IsNotNull(() => databaseService);
            Argument.IsNotNull(() => applicationService);

            _applicationService = applicationService;
            _databaseService = databaseService;
            Plugins = new TrackableCollection<Plugin>();
        }

        public async Task LoadPlugins()
        {
            var plugins = await _databaseService.Context.Plugins.Include(plugin => plugin.AdditionalBankFiles)
                .Include(plugin => plugin.PluginLocation).ToListAsync();
            Plugins.SynchronizeCollection(plugins);
        }

        public IRemoteVstService GetVstService()
        {
            return _applicationService.NewProcessPool.GetVstService();
        }

        public async Task SavePlugins()
        {
            await _databaseService.Context.SaveChangesAsync();
        }

        public byte[] GetPresetData(Preset preset)
        {
            return _databaseService.Context.GetPresetData(preset);
        }

        public IRemotePluginInstance GetRemotePluginInstance(Plugin plugin, bool backgroundProcessing = true)
        {
            return _applicationService.NewProcessPool.GetRemotePluginInstance(plugin, backgroundProcessing);
        }

        public List<PluginLocation> GetPluginLocations(Plugin plugin)
        {
            return _databaseService.Context.GetPluginLocations(plugin);
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
        public TrackableCollection<Plugin> Plugins { get; set; }

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