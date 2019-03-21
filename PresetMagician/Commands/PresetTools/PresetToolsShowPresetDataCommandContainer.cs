using System.Collections.Specialized;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetToolsShowPresetDataCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IUIVisualizerService _uiVisualizerService;

        public PresetToolsShowPresetDataCommandContainer(ICommandManager commandManager, IServiceLocator serviceLocator)
            : base(Commands.PresetTools.ShowPresetData, commandManager, serviceLocator)
        {
            _uiVisualizerService = Catel.IoC.ServiceLocator.Default.ResolveType<IUIVisualizerService>();

            _globalFrontendService.SelectedPresets.CollectionChanged += OnSelectedPresetsListChanged;
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _globalFrontendService.SelectedPresets.Count == 1;
        }

        private void OnSelectedPresetsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }


        protected override async Task ExecuteAsync(object parameter)
        {
            await _uiVisualizerService.ShowDialogAsync<PresetDataViewModel>(_globalFrontendService
                .SelectedExportPreset);
        }
    }
}