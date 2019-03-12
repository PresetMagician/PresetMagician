using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel;
using Catel.Collections;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Interfaces;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using PresetMagician.Core.Models;

namespace PresetMagician.ViewModels
{
    public class VstPluginsViewModel : ViewModelBase
    {
        private readonly IVstService _vstService;
        private readonly ICommandManager _commandManager;

        public VstPluginsViewModel(ICustomStatusService statusService, IPleaseWaitService pleaseWaitService,
            IRuntimeConfigurationService runtimeConfigurationService, IServiceLocator serviceLocator,
            IViewModelFactory viewModelFactory, IUIVisualizerService uiVisualizerService, IVstService vstService,
            ICommandManager commandManager)
        {
            Argument.IsNotNull(() => vstService);
            _vstService = vstService;
            _commandManager = commandManager;


            Plugins = vstService.Plugins;
            SelectedPlugins = vstService.SelectedPlugins;
            ApplicationState = runtimeConfigurationService.ApplicationState;
            serviceLocator.RegisterInstance(this);

            Plugins.CollectionChanged += PluginsOnCollectionChanged;

            _vstService.SelectedPluginChanged += VstServiceOnSelectedPluginChanged;

            Title = "VST Plugins";
        }
        
        protected override Task OnClosedAsync(bool? result)
        {
            return base.OnClosedAsync(result);
        }

        private void VstServiceOnSelectedPluginChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(SelectedPlugin));
        }

        private void PluginsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(HasPlugins));
        }

        public bool HasPlugins => Plugins.Count > 0;

        public Plugin SelectedPlugin
        {
            get => _vstService.SelectedPlugin;
            set => _vstService.SelectedPlugin = value;
        }

        public ObservableCollection<Plugin> Plugins { get; }
        public FastObservableCollection<Plugin> SelectedPlugins { get; }
        public ApplicationState ApplicationState { get; private set; }
    }
}