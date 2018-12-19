using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.MVVM;
using Catel.Threading;
using PresetMagicianShell.Extensions;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagicianShell
{
    // ReSharper disable once UnusedMember.Global
    public class PluginRefreshPluginsCommandContainer : CommandContainerBase
    {
        private readonly IApplicationService _applicationService;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly IVstService _vstService;

        public PluginRefreshPluginsCommandContainer(ICommandManager commandManager,
            IVstService vstService, IRuntimeConfigurationService runtimeConfigurationService,
            IApplicationService applicationService)
            : base(Commands.Plugin.RefreshPlugins, commandManager)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => applicationService);

            _runtimeConfigurationService = runtimeConfigurationService;
            _vstService = vstService;
            _applicationService = applicationService;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var vstPluginDLLFiles = new ObservableCollection<string>();

            _applicationService.StartApplicationOperation(this, "Scanning VST directories for plugins",
                _runtimeConfigurationService.RuntimeConfiguration.VstDirectories.Count);

            var cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;
            var vstDirectories = _runtimeConfigurationService.RuntimeConfiguration.VstDirectories;

            await TaskHelper.Run(() =>
            {
                var dllFiles = new ObservableCollection<string>();

                foreach (var i in vstDirectories)
                {
                    _applicationService.UpdateApplicationOperationStatus(
                        vstDirectories.IndexOf(i),
                        $"Scanning {i.Path}");

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        dllFiles = _vstService.VstHost.EnumeratePlugins(i.Path);
                    }
                    catch (Exception e)
                    {
                        _applicationService.AddApplicationOperationError($"Unable to scan {i.Path} because of {e.Message}");
                    }

                    if (dllFiles.Count > 0)
                    {
                        vstPluginDLLFiles.AddRange(dllFiles);
                    }
                }
            }, true, cancellationToken);

            var plugins = _vstService.Plugins;

            using (plugins.SuspendChangeNotifications())
            {
                plugins.RemoveAll(item => !vstPluginDLLFiles.Contains(item.DllPath));

                foreach (var dllPath in vstPluginDLLFiles)
                {
                    var foundPlugin = (from plugin in plugins where plugin.DllPath == dllPath select plugin)
                        .FirstOrDefault();

                    if (foundPlugin == null)
                    {
                        plugins.Add(new Plugin
                        {
                            DllPath = dllPath
                        });
                    }
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                _applicationService.StopApplicationOperation("VST directory scan aborted.");
            }
            else
            {
                _applicationService.StopApplicationOperation("VST directory scan completed.");
            }
            
        }
    }
}