using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Data;
using Catel;
using Catel.Collections;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Drachenkatze.PresetMagician.VSTHost.VST;
using NuGet;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;

namespace PresetMagician.ViewModels
{
    public class VstPluginsViewModel : ViewModelBase
    {
        
        private readonly IVstService _vstService;
        private readonly ICommandManager _commandManager;

        public VstPluginsViewModel(ICustomStatusService statusService, IPleaseWaitService pleaseWaitService,
            IRuntimeConfigurationService runtimeConfigurationService, IServiceLocator serviceLocator,
            IViewModelFactory viewModelFactory, IUIVisualizerService uiVisualizerService, IVstService vstService, ICommandManager commandManager)
        {
            Argument.IsNotNull(() => vstService);
            _vstService = vstService;
            _commandManager = commandManager;
          

            Plugins = vstService.Plugins;
            SelectedPlugins = vstService.SelectedPlugins;
            ApplicationState = runtimeConfigurationService.ApplicationState;
            serviceLocator.RegisterInstance(this);

           
            var pView = CollectionViewSource.GetDefaultView(Plugins);

            pView.SortDescriptions.Add(new SortDescription("Enabled", ListSortDirection.Descending));
            pView.SortDescriptions.Add(new SortDescription("IsSupported", ListSortDirection.Descending));
            pView.SortDescriptions.Add(new SortDescription("PluginName", ListSortDirection.Ascending));




            var productview = (ICollectionViewLiveShaping) CollectionViewSource.GetDefaultView(pView);
            productview.IsLiveSorting = true;

            Title = "VST Plugins";
        }

        protected override async Task InitializeAsync()
        {
            await TaskHelper.Run(() =>
            { _commandManager.ExecuteCommand(Commands.Plugin.RefreshPlugins); },true);
            await base.InitializeAsync();
        }

        public Plugin SelectedPlugin
        {
            get => _vstService.SelectedPlugin;
            set => _vstService.SelectedPlugin = value;
        }


        public FastObservableCollection<Plugin> Plugins { get; }
        public FastObservableCollection<Plugin> SelectedPlugins { get; }
        public ApplicationState ApplicationState { get; private set; }

      

    }
}