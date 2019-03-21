using Catel.Collections;
using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

namespace PresetMagician.ViewModels
{
    public class PresetExportListViewModel : ViewModelBase
    {
        private readonly GlobalFrontendService _globalFrontendService;

        public PresetExportListViewModel()
        {
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();

            PresetExportList = _globalFrontendService.PresetExportList;
            ApplicationState = _globalFrontendService.ApplicationState;

            ServiceLocator.Default.RegisterInstance(this);

            SelectedPresets = _globalFrontendService.SelectedPresets;

            Title = "Preset Export List";
        }

        public Preset SelectedExportPreset
        {
            get => _globalFrontendService.SelectedExportPreset;
            set => _globalFrontendService.SelectedExportPreset = value;
        }

        public FastObservableCollection<Preset> SelectedPresets { get; }
        public FastObservableCollection<Preset> PresetExportList { get; }
        public ApplicationState ApplicationState { get; }
    }
}