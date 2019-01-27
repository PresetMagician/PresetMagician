using System.Collections.Generic;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Threading;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class ApplicationApplyConfigurationCommandContainer : CommandContainerBase
    {
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly ICommandManager _commandManager;

        public ApplicationApplyConfigurationCommandContainer(ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService)
            : base(Commands.Application.ApplyConfiguration, commandManager)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);
            _runtimeConfigurationService = runtimeConfigurationService;
            _commandManager = commandManager;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var currentConfiguration = _runtimeConfigurationService.RuntimeConfiguration;
            var newConfiguration = _runtimeConfigurationService.EditableConfiguration;

            List<string> commandsList = new List<string>();

            if (!_runtimeConfigurationService.IsConfigurationValueEqual(currentConfiguration.VstDirectories,
                newConfiguration.VstDirectories))
            {
                commandsList.Add(Commands.Plugin.RefreshPlugins);
            }

            _runtimeConfigurationService.ApplyEditableConfiguration();
            _runtimeConfigurationService.Save();
            await TaskHelper.Run(() =>
            {
                foreach (var command in commandsList)
                {
                    _commandManager.ExecuteCommand(command);
                }
            }, true);
        }
    }
}