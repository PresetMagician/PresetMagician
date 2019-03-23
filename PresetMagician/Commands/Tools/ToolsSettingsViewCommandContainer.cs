using System.Threading.Tasks;
using Catel.IoC;
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
        public ToolsSettingsViewCommandContainer(
            ICommandManager commandManager,IServiceLocator serviceLocator)
            : base(Commands.Tools.SettingsView, nameof(SettingsViewModel), false, commandManager, serviceLocator)
        {
        }

        protected override void OnBeforeShowDialog(IViewModel viewModel, object parameter)
        {
            var vm = viewModel as SettingsViewModel;
            vm.SelectedTabTitle = parameter as string;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            _runtimeConfigurationService.CreateEditableConfiguration();
            await base.ExecuteAsync(parameter);
        }
    }
}