using Catel.IoC;
using Catel.MVVM;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetExportClearListCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly GlobalFrontendService _globalFrontendService;

        public PresetExportClearListCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.PresetExport.ClearList, commandManager, runtimeConfigurationService)
        {
            _globalFrontendService = ServiceLocator.Default.ResolveType<GlobalFrontendService>();
        }

        protected override void Execute(object parameter)
        {
            _globalFrontendService.SelectedPresets.Clear();
            _globalFrontendService.PresetExportList.Clear();
        }
    }
}