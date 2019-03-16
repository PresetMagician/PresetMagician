using Catel.MVVM;
using Catel.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class ToolsEditTypesCharacteristicsCommandContainer : AbstractOpenDialogCommandContainer
    {
        public ToolsEditTypesCharacteristicsCommandContainer(ICommandManager commandManager, IUIVisualizerService uiVisualizerService,
            IViewModelFactory viewModelFactory, IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.Tools.EditTypesCharacteristics, nameof(TypesCharacteristicsViewModel), false, commandManager, uiVisualizerService, runtimeConfigurationService,
                viewModelFactory)
        {
        }
    }
}