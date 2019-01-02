using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Threading;
using PresetMagician.Extensions;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginRefreshPluginsCommandContainer : CommandContainerBase
    {
        private static readonly ILog _logger = LogManager.GetCurrentClassLogger();

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
            var vstDirectories = (from vstDirectory in _runtimeConfigurationService.RuntimeConfiguration.VstDirectories
                where vstDirectory.Active
                select vstDirectory).ToList();

            _applicationService.StartApplicationOperation(this, "Scanning VST directories for plugins",
                vstDirectories.Count);

            var cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;

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
                        var plugin = new Plugin
                        {
                            DllPath = dllPath
                        };
                        
                        plugins.Add(plugin);
                        
                        var foundCachedPlugin = (from cachedPlugin in _vstService.CachedPlugins where cachedPlugin.DllPath == dllPath select cachedPlugin)
                            .FirstOrDefault();

                        if (foundCachedPlugin != null)
                        {
                            plugin.PluginType = foundCachedPlugin.PluginType;
                            plugin.PluginId = foundCachedPlugin.PluginId;
                            plugin.PluginVendor = foundCachedPlugin.PluginVendor;
                            plugin.PluginName = foundCachedPlugin.PluginName;
                            plugin.Reported = foundCachedPlugin.Reported;
                            plugin.Configuration = foundCachedPlugin.Configuration;
                        }
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