using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetToolsShowPresetDataCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IVstService _vstService;
        private readonly IUIVisualizerService _uiVisualizerService;

        public PresetToolsShowPresetDataCommandContainer(ICommandManager commandManager, IVstService vstService,
            IUIVisualizerService uiVisualizerService, IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.PresetTools.ShowPresetData, commandManager, runtimeConfigurationService)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => uiVisualizerService);

            _vstService = vstService;
            _uiVisualizerService = uiVisualizerService;

            _vstService.SelectedPresets.CollectionChanged += OnSelectedPresetsListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) &&  _vstService.SelectedPresets.Count == 1;
        }

        private void OnSelectedPresetsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            await _uiVisualizerService.ShowDialogAsync<PresetDataViewModel>(_vstService.SelectedExportPreset);
        }
    }
}