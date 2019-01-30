using System.Threading.Tasks;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class ToolsSettingsViewCommandContainer : AbstractOpenDialogCommandContainer
    {
        public ToolsSettingsViewCommandContainer(IRuntimeConfigurationService runtimeConfigurationService,
            ICommandManager commandManager, IUIVisualizerService uiVisualizerService,
            IViewModelFactory viewModelFactory)
            : base(Commands.Tools.SettingsView, nameof(SettingsViewModel), false, commandManager, uiVisualizerService, runtimeConfigurationService,
                viewModelFactory)
        {
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            _runtimeConfigurationService.CreateEditableConfiguration();
            await base.ExecuteAsync(parameter);
        }
    }
}