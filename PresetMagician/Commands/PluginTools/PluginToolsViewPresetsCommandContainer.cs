using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Catel;
using Catel.Data;
using Catel.MVVM;
using Catel.Services;
using MethodTimer;
using PresetMagician.Core.Interfaces;
using PresetMagician.Extensions;
using PresetMagician.Helpers;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;
using PresetMagician.Core.Models;
// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginToolsViewPresetsCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IVstService _vstService;
        private Plugin _previouslySelectedPlugin;
        private readonly IViewModelFactory _viewModelFactory;

        public PluginToolsViewPresetsCommandContainer(ICommandManager commandManager, IVstService vstService,
            IUIVisualizerService uiVisualizerService,IRuntimeConfigurationService runtimeConfigurationService,
            IViewModelFactory viewModelFactory
        )
            : base(Commands.PluginTools.ViewPresets, commandManager,runtimeConfigurationService, true)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => uiVisualizerService);

            _vstService = vstService;
            _vstService.SelectedPlugins.CollectionChanged += OnSelectedPluginsListChanged;
            _uiVisualizerService = uiVisualizerService;
            _viewModelFactory = viewModelFactory;
        }
        
     
        private void SelectedPluginOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            InvalidateCommand();
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _vstService.SelectedPlugins.Count > 0 &&
                   _vstService.SelectedPlugin != null;
        }

        private void OnSelectedPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            if (_previouslySelectedPlugin != null)
            {
                _previouslySelectedPlugin.PropertyChanged -= SelectedPluginOnPropertyChanged;
            }

            if (_vstService.SelectedPlugin != null)
            {
                _vstService.SelectedPlugin.PropertyChanged += SelectedPluginOnPropertyChanged;
                _previouslySelectedPlugin = _vstService.SelectedPlugin;
            }

            InvalidateCommand();
        }


        [Time]
        protected override async Task ExecuteAsync(object parameter)
        {
            /*_vstService.SelectedPlugin.ClearIsDirtyOnAllChildsSuspended();
            _vstService.SelectedPlugin.ClearDirtyFlag();*/

            var presetsViewModel =
                _viewModelFactory.CreateViewModel<VstPluginPresetsViewModel>(_vstService.SelectedPlugin);
            AvalonDockHelper.CreateDocument<VstPluginPresetsViewModel>(presetsViewModel, _vstService.SelectedPlugin,
                activateDocument: true, isClosable: true, shouldTrackDirty: true);
            
            //_vstService.SelectedPlugin.BeginEdit();
            
        }

        
    }
}