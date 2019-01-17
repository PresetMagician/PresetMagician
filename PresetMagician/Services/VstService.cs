using System;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Threading.Tasks;
using Catel;
using Catel.Collections;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagician.Services.Interfaces;
using SharedModels;

namespace PresetMagician.Services
{
    public class VstService : IVstService
    {
        private IDatabaseService _databaseService;
        public VstService(IDatabaseService databaseService)
        {
            Argument.IsNotNull(() => databaseService);

            _databaseService = databaseService;
            _databaseService.Context.Plugins.Include(plugin => plugin.AdditionalBankFiles).Load();

            Plugins = _databaseService.Context.Plugins.Local;
            
            VstHost = new Drachenkatze.PresetMagician.VSTHost.VST.VstHost();
        }

        public async Task SavePlugins()
        {
            await _databaseService.Context.SaveChangesAsync();
        }

        public byte[] GetPresetData(Preset preset)
        {
            return _databaseService.Context.GetPresetData(preset);
        }

        public Drachenkatze.PresetMagician.VSTHost.VST.VstHost VstHost { get; set; }


        public FastObservableCollection<Plugin> SelectedPlugins { get; } = new FastObservableCollection<Plugin>();
        public ObservableCollection<Plugin> Plugins { get; }
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