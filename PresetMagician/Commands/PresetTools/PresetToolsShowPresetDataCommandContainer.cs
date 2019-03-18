using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetToolsShowPresetDataCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly GlobalFrontendService _globalFrontendService;

        public PresetToolsShowPresetDataCommandContainer(ICommandManager commandManager,IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.PresetTools.ShowPresetData, commandManager, runtimeConfigurationService)
        {
           
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();
            _uiVisualizerService = ServiceLocator.Default.ResolveType<IUIVisualizerService>();

            _globalFrontendService.SelectedPresets.CollectionChanged += OnSelectedPresetsListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) &&  _globalFrontendService.SelectedPresets.Count == 1;
        }

        private void OnSelectedPresetsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            await _uiVisualizerService.ShowDialogAsync<PresetDataViewModel>(_globalFrontendService.SelectedExportPreset);
        }
    }
}