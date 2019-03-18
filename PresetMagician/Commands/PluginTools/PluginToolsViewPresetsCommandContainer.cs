using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using MethodTimer;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;
using PresetMagician.Helpers;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsViewPresetsCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly GlobalFrontendService _globalFrontendService;
        private Plugin _previouslySelectedPlugin;
        private readonly IViewModelFactory _viewModelFactory;

        public PluginToolsViewPresetsCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService,
            IViewModelFactory viewModelFactory
        )
            : base(Commands.PluginTools.ViewPresets, commandManager, runtimeConfigurationService, true)
        {
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();
            _globalFrontendService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
            _viewModelFactory = viewModelFactory;
        }


        private void SelectedPluginOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InvalidateCommand();
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _globalFrontendService.SelectedPlugins.Count > 0 &&
                   _globalFrontendService.SelectedPlugin != null;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            if (_previouslySelectedPlugin != null)
            {
                _previouslySelectedPlugin.PropertyChanged -= SelectedPluginOnPropertyChanged;
            }

            if (_globalFrontendService.SelectedPlugin != null)
            {
                _globalFrontendService.SelectedPlugin.PropertyChanged += SelectedPluginOnPropertyChanged;
                _previouslySelectedPlugin = _globalFrontendService.SelectedPlugin;
            }

            InvalidateCommand();
        }


        [Time]
        protected override async Task ExecuteAsync(object parameter)
        {
            /*_vstService.SelectedPlugin.ClearIsDirtyOnAllChildsSuspended();
            _vstService.SelectedPlugin.ClearDirtyFlag();*/

            var presetsViewModel =
                _viewModelFactory.CreateViewModel<VstPluginPresetsViewModel>(_globalFrontendService.SelectedPlugin);
            AvalonDockHelper.CreateDocument<VstPluginPresetsViewModel>(presetsViewModel,
                _globalFrontendService.SelectedPlugin,
                activateDocument: true, isClosable: true, shouldTrackDirty: true);

            //_vstService.SelectedPlugin.BeginEdit();
        }
    }
}