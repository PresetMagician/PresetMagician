using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Threading.Tasks;
using Catel;
using Catel.Collections;
using PresetMagician.ProcessIsolation;
using PresetMagician.Services.Interfaces;
using SharedModels;

namespace PresetMagician.Services
{
    public class VstService : IVstService
    {
        private readonly IDatabaseService _databaseService;
        private readonly IsolatedProcess _frontendProcess = new IsolatedProcess();

        private readonly Dictionary<Plugin, IRemotePluginInstance> _pluginInstances =
            new Dictionary<Plugin, IRemotePluginInstance>();

        public VstService(IDatabaseService databaseService)
        {
            Argument.IsNotNull(() => databaseService);

            _databaseService = databaseService;
            _databaseService.Context.Plugins.Include(plugin => plugin.AdditionalBankFiles).Load();
            Plugins = _databaseService.Context.Plugins.Local;
        }

        public async Task SavePlugins()
        {
            await _databaseService.Context.SaveChangesAsync();
        }

        public byte[] GetPresetData(Preset preset)
        {
            return _databaseService.Context.GetPresetData(preset);
        }

        public async Task<IRemotePluginInstance> GetRemotePluginInstance(Plugin plugin)
        {
            return await ProcessPool.GetRemotePluginInstance(plugin);
        }

        public IRemotePluginInstance GetInteractivePluginInstance(Plugin plugin)
        {
            if (!_pluginInstances.ContainsKey(plugin))
            {
                _pluginInstances.Add(plugin, _frontendProcess.GetRemotePluginInstance(plugin));
            }

            return _pluginInstances[plugin];
        }


        public FastObservableCollection<Plugin> SelectedPlugins { get; } = new FastObservableCollection<Plugin>();
        public ObservableCollection<Plugin> Plugins { get; set; }
        public FastObservableCollection<Plugin> CachedPlugins { get; } = new FastObservableCollection<Plugin>();

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
                SelectedPluginChanged.SafeInvoke(this);
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
                SelectedExportPresetChanged.SafeInvoke(this);
            }
        }

        public event EventHandler SelectedExportPresetChanged;

        #endregion
    }
}