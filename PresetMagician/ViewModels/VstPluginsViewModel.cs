using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel.Collections;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    public class VstPluginsViewModel : ViewModelBase
    {
        private readonly GlobalFrontendService _globalFrontendService;

        public VstPluginsViewModel(
            IRuntimeConfigurationService runtimeConfigurationService
        )
        {
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();
            var globalService = ServiceLocator.Default.ResolveType<GlobalService>();


            ApplicationOperationStatus = ServiceLocator.Default.ResolveType<IApplicationService>()
                .GetApplicationOperationStatus();
            Plugins = globalService.Plugins;
            SelectedPlugins = _globalFrontendService.SelectedPlugins;
            ServiceLocator.Default.RegisterInstance(this);

            Plugins.CollectionChanged += PluginsOnCollectionChanged;

            _globalFrontendService.SelectedPluginChanged += VstServiceOnSelectedPluginChanged;

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
            get => _globalFrontendService.SelectedPlugin;
            set => _globalFrontendService.SelectedPlugin = value;
        }

        public ObservableCollection<Plugin> Plugins { get; }
        public FastObservableCollection<Plugin> SelectedPlugins { get; }
        public ApplicationOperationStatus ApplicationOperationStatus { get; }
    }
}