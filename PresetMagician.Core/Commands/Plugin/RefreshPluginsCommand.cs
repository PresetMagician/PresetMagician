using System;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Catel;
using Catel.Collections;
using Catel.IoC;
using PresetMagician.Core.Services;

namespace PresetMagician.Core.Commands.Plugin
{
    public class RefreshPluginsCommand: CommandBase
    {
        private readonly PluginService _pluginService;
        private readonly DataPersisterService _dataPersisterService;

        public RefreshPluginsCommand(IServiceLocator serviceLocator): base(serviceLocator)
        {
            _pluginService = ServiceLocator.ResolveType<PluginService>();
            _dataPersisterService = ServiceLocator.ResolveType<DataPersisterService>();
        }

        public override async Task ExecuteAsync()
        {
            var vstDirectories = (from vstDirectory in GlobalService.RuntimeConfiguration.VstDirectories
                where vstDirectory.Active
                select vstDirectory).ToList();

            ApplicationService.StartApplicationOperation(this, "Scanning VST directories for plugins",
                vstDirectories.Count);

            var progress = ApplicationService.GetApplicationProgress();
            try
            {
                var vstPluginDLLFiles =
                    _pluginService.GetPluginDlls(vstDirectories, progress);

                var plugins = GlobalService.Plugins;
                ApplicationService.StopApplicationOperation("VST directory scan completed.");

                if (!progress.CancellationToken.IsCancellationRequested)
                {
                    ApplicationService.StartApplicationOperation(this, "Adding / Verifying plugins",
                        plugins.Count);
                    var pluginsToAdd = await _pluginService.VerifyPlugins(plugins, vstPluginDLLFiles, ApplicationService.GetApplicationProgress())
                        .ConfigureAwait(false);

                    using (plugins.SuspendChangeNotifications())
                    {
                        plugins.AddRange(pluginsToAdd);
                    }

                    ApplicationService.StopApplicationOperation("Verifying plugins complete");
                }

                ApplicationService.StartApplicationOperation(this, "Saving plugins",
                    vstDirectories.Count);
                _dataPersisterService.Save();
                ApplicationService.StopApplicationOperation("Saving plugins complete");
            }
            catch (Exception e)
            {
                ApplicationService.AddApplicationOperationError("An unexpected error occured: " + e.Message);
                LogTo.Debug(e.StackTrace);
            }

            ApplicationService.StopApplicationOperation(progress.CancellationToken.IsCancellationRequested
                ? "VST directory scan aborted."
                : "VST directory scan completed.");
        }
    }
}