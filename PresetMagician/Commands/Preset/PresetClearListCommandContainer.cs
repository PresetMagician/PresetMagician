using Catel;
using Catel.MVVM;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PresetClearListCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IVstService _vstService;

        public PresetClearListCommandContainer(ICommandManager commandManager, IVstService vstService, IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.Preset.ClearList, commandManager, runtimeConfigurationService)
        {
            Argument.IsNotNull(() => vstService);

            _vstService = vstService;
        }

        protected override void Execute(object parameter)
        {
            _vstService.SelectedPresets.Clear();
            _vstService.PresetExportList.Clear();
        }
    }
}