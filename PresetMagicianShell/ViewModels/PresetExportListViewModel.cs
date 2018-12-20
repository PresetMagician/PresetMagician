using Catel;
using Catel.Collections;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Drachenkatze.PresetMagician.VendorPresetParser;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;

namespace PresetMagicianShell.ViewModels
{
    public class PresetExportListViewModel : ViewModelBase
    {
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();
        private readonly IVstService _vstService;

        public PresetExportListViewModel(ICustomStatusService statusService, 
            IRuntimeConfigurationService runtimeConfigurationService, IServiceLocator serviceLocator,
            IVstService vstService)
        {
            Argument.IsNotNull(() => statusService);
            Argument.IsNotNull(() => vstService);
            _vstService = vstService;

            PresetExportList = vstService.PresetExportList;
            ApplicationState = runtimeConfigurationService.ApplicationState;
            serviceLocator.RegisterInstance(this);

            Title = "Preset Export List";
        }

        public Preset SelectedExportPreset
        {
            get => _vstService.SelectedExportPreset;
            set => _vstService.SelectedExportPreset = value;
        }


        public FastObservableCollection<Preset> PresetExportList { get; }
        public ApplicationState ApplicationState { get; }
    }
}