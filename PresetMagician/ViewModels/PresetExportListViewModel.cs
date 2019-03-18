using Catel;
using Catel.Collections;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using PresetMagician.Core.Interfaces;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

namespace PresetMagician.ViewModels
{
    public class PresetExportListViewModel : ViewModelBase
    {
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();
        private readonly GlobalFrontendService _globalFrontendService;
        
        public PresetExportListViewModel(IRuntimeConfigurationService runtimeConfigurationService)
        {
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();
            
            PresetExportList = _globalFrontendService.PresetExportList;
            ApplicationState = runtimeConfigurationService.ApplicationState;
            
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