using System;
using Catel;
using Catel.Collections;
using Catel.IoC;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.ViewModels;

namespace PresetMagicianShell.Services
{
    public class VstService : IVstService
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        public VstHost VstHost { get; set; }
        public VstService(IServiceLocator serviceLocator, IRuntimeConfigurationService runtimeConfigurationService)
        {
            Argument.IsNotNull(() => serviceLocator);
            Argument.IsNotNull(() => runtimeConfigurationService);

            _serviceLocator = serviceLocator;
            _runtimeConfigurationService = runtimeConfigurationService;
            VstHost = new VstHost();

            Plugins = _runtimeConfigurationService.RuntimeConfiguration.Plugins;
        }


        public FastObservableCollection<Plugin> SelectedPlugins { get; } = new FastObservableCollection<Plugin>();
        public FastObservableCollection<Plugin> Plugins { get; private set; } 

       
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
    }
}