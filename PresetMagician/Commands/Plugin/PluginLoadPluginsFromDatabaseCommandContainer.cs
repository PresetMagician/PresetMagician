using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anotar.Catel;
using Catel;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Models.Settings;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using SharedModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginLoadPluginsFromDatabaseCommandContainer : CommandContainerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IVstService _vstService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IMessageService _messageService;
        private readonly IDatabaseService _databaseService;

        public PluginLoadPluginsFromDatabaseCommandContainer(ICommandManager commandManager,
            IVstService vstService, IRuntimeConfigurationService runtimeConfigurationService,
            IApplicationService applicationService, IDispatcherService dispatcherService,
            IMessageService messageService, IDatabaseService databaseService)
            : base(Commands.Plugin.LoadPluginsFromDatabase, commandManager)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => dispatcherService);
            Argument.IsNotNull(() => messageService);
            Argument.IsNotNull(() => databaseService);

            _dispatcherService = dispatcherService;
            _vstService = vstService;
            _applicationService = applicationService;
            _messageService = messageService;
            _databaseService = databaseService;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            await _vstService.LoadPlugins();
        }

    }
}