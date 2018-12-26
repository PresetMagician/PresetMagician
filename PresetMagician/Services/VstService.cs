using System;
using Catel;
using Catel.Collections;
using Catel.IoC;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.Services
{
    public class VstService : IVstService
    {
        private readonly IServiceLocator _serviceLocator;

        public VstService(IServiceLocator serviceLocator)
        {
            Argument.IsNotNull(() => serviceLocator);

            _serviceLocator = serviceLocator;
            VstHost = new VstHost();
        }

        public VstHost VstHost { get; set; }


        public FastObservableCollection<Plugin> SelectedPlugins { get; } = new FastObservableCollection<Plugin>();
        public FastObservableCollection<Plugin> Plugins { get; } = new FastObservableCollection<Plugin>();
        
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