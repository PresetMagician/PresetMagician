using System;
using Catel;
using Catel.Collections;
using Catel.IoC;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.ViewModels;

namespace PresetMagicianShell.Services
{
    public class VstService : IVstService
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;

        public VstService(IServiceLocator serviceLocator, IRuntimeConfigurationService runtimeConfigurationService)
        {
            Argument.IsNotNull(() => serviceLocator);
            Argument.IsNotNull(() => runtimeConfigurationService);

            _serviceLocator = serviceLocator;
            _runtimeConfigurationService = runtimeConfigurationService;

            Plugins = _runtimeConfigurationService.RuntimeConfiguration.Plugins;
        }


        public FastObservableCollection<Plugin> SelectedPlugins { get; } = new FastObservableCollection<Plugin>();
        public FastObservableCollection<Plugin> Plugins { get; private set; } 

        public void RefreshPluginList()
        {
            var vstPluginViewModel = _serviceLocator.ResolveType<VstPluginsViewModel>();
            vstPluginViewModel.RefreshPluginList.Execute();
        }

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