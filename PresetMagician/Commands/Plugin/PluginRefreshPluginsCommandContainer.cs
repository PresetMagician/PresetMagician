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
using PresetMagician.Core.Interfaces;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Core.Services;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginRefreshPluginsCommandContainer : ThreadedApplicationNotBusyCommandContainer
    {
        private readonly IApplicationService _applicationService;
        private readonly IVstService _vstService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IMessageService _messageService;
        private readonly PluginService _pluginService;

        public PluginRefreshPluginsCommandContainer(ICommandManager commandManager,
            IVstService vstService, IRuntimeConfigurationService runtimeConfigurationService,
            IApplicationService applicationService, IDispatcherService dispatcherService,
            PluginService pluginService,
            IMessageService messageService)
            : base(Commands.Plugin.RefreshPlugins, commandManager, runtimeConfigurationService)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => dispatcherService);
            Argument.IsNotNull(() => pluginService);
            Argument.IsNotNull(() => messageService);

            _dispatcherService = dispatcherService;
            _vstService = vstService;
            _pluginService = pluginService;
            _applicationService = applicationService;
            _messageService = messageService;
        }


        protected override async Task ExecuteThreaded(object parameter)
        {
            var vstDirectories = (from vstDirectory in _runtimeConfigurationService.RuntimeConfiguration.VstDirectories
                where vstDirectory.Active
                select vstDirectory).ToList();

            _applicationService.StartApplicationOperation(this, "Scanning VST directories for plugins",
                vstDirectories.Count);

            var progress = _applicationService.CreateApplicationProgress();
            try
            {
                 var vstPluginDLLFiles =
                    await TaskHelper.Run(() => _pluginService.GetPluginDlls(vstDirectories, progress), false, progress.CancellationToken);

                var plugins = _vstService.Plugins;
                _applicationService.StopApplicationOperation("VST directory scan completed.");

                if (!progress.CancellationToken.IsCancellationRequested)
                {
                    _applicationService.StartApplicationOperation(this, "Verifying plugins",
                        plugins.Count);
                    var pluginsToAdd =
                        await TaskHelper.Run(() => _pluginService.VerifyPlugins(plugins, vstPluginDLLFiles, progress), false, progress.CancellationToken);

                    foreach (var plugin in pluginsToAdd)
                    {
                        _dispatcherService.Invoke(() => plugins.Add(plugin));
                    }

                    _applicationService.StopApplicationOperation("Verifying plugins complete");
                }

                _vstService.Save();
            }
            catch (Exception e)
            {
                _applicationService.AddApplicationOperationError("An unexpected error occured: "+e.Message);
                LogTo.Debug(e.StackTrace);
            }

            _applicationService.StopApplicationOperation(progress.CancellationToken.IsCancellationRequested
                ? "VST directory scan aborted."
                : "VST directory scan completed.");
        }
    }
}