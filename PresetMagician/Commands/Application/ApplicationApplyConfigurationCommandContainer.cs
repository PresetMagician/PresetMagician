using System.Collections.Generic;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using Catel.Threading;
using PresetMagician.Core.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class ApplicationApplyConfigurationCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly ICommandManager _commandManager;
        private readonly GlobalService _globalService;

        public ApplicationApplyConfigurationCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(Commands.Application.ApplyConfiguration, commandManager, serviceLocator)
        {
            _globalService = ServiceLocator.ResolveType<GlobalService>();
            _commandManager = commandManager;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var currentConfiguration = _globalService.RuntimeConfiguration;
            var newConfiguration = RuntimeConfigurationService.EditableConfiguration;

            var commandsList = new List<string>();

            if (!RuntimeConfigurationService.IsConfigurationValueEqual(currentConfiguration.VstDirectories,
                newConfiguration.VstDirectories))
            {
                commandsList.Add(Commands.Plugin.RefreshPlugins);
            }

            RuntimeConfigurationService.ApplyEditableConfiguration();
            RuntimeConfigurationService.Save();
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