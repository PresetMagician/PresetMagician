using System.Threading.Tasks;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Helpers;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class ToolsSettingsViewCommandContainer : AbstractOpenDialogCommandContainer
    {
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;

        public ToolsSettingsViewCommandContainer(IRuntimeConfigurationService runtimeConfigurationService,ICommandManager commandManager, IUIVisualizerService uiVisualizerService, IViewModelFactory viewModelFactory)
            : base(Commands.Tools.SettingsView, nameof(SettingsViewModel),commandManager, uiVisualizerService, viewModelFactory)
        {
            _runtimeConfigurationService = runtimeConfigurationService;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            _runtimeConfigurationService.CreateEditableConfiguration();
            await base.ExecuteAsync(parameter);
        }
    }
}