using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Anotar.Catel;
using Catel;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Drachenkatze.PresetMagician.Utils;
using PresetMagician.Extensions;
using PresetMagician.Models;
using PresetMagician.ProcessIsolation;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using SharedModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginRefreshPluginsCommandContainer : ApplicationNotBusyCommandContainer
    {
        private readonly IApplicationService _applicationService;
        private readonly IVstService _vstService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IMessageService _messageService;
        private readonly IDatabaseService _databaseService;

        public PluginRefreshPluginsCommandContainer(ICommandManager commandManager,
            IVstService vstService, IRuntimeConfigurationService runtimeConfigurationService,
            IApplicationService applicationService, IDispatcherService dispatcherService,
            IMessageService messageService, IDatabaseService databaseService)
            : base(Commands.Plugin.RefreshPlugins, commandManager, runtimeConfigurationService)
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
            var vstPluginDLLFiles = new ObservableCollection<string>();
            var vstDirectories = (from vstDirectory in _runtimeConfigurationService.RuntimeConfiguration.VstDirectories
                where vstDirectory.Active
                select vstDirectory).ToList();

            _applicationService.StartApplicationOperation(this, "Scanning VST directories for plugins",
                vstDirectories.Count);

            var cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;

            await TaskHelper.Run(() =>
            {
                var dllFiles = new List<string>();

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
                        dllFiles = VstUtils.EnumeratePlugins(i.Path);
                    }
                    catch (Exception e)
                    {
                        _applicationService.AddApplicationOperationError(
                            $"Unable to scan {i.Path} because of {e.Message}");
                    }

                    if (dllFiles.Count > 0)
                    {
                        vstPluginDLLFiles.AddRange(dllFiles);
                    }
                }
            }, true, cancellationToken);

            var plugins = _vstService.Plugins;
            _applicationService.StopApplicationOperation("VST directory scan completed.");
            _applicationService.StartApplicationOperation(this, "Verifying plugins",
                plugins.Count);

            var pluginsToAdd = new List<Plugin>();

            var remoteFileService = ProcessPool.GetRemoteFileService().Result;

            await TaskHelper.Run(() =>
            {
                foreach (var plugin in plugins)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (plugin.PluginLocation != null)
                    {
                        _applicationService.UpdateApplicationOperationStatus(
                            plugins.IndexOf(plugin),
                            $"Verifying {plugin.PluginLocation.DllPath}");

                        if (remoteFileService.Exists(plugin.PluginLocation.DllPath))
                        {
                            var currentHash = remoteFileService.GetHash(plugin.PluginLocation.DllPath);

                            if (plugin.PluginLocation.DllHash !=
                                currentHash)

                            {
                                // Plugin DLL has changed. That could mean:
                                // - New Version
                                // - Completely different plugin
                                // In any case, we need to create a new plugin waiting to be scanned. After scanning,
                                // the plugin location can be appended to this plugin again.

                                var newPlugin = new Plugin
                                {
                                    PluginLocation = new PluginLocation
                                    {
                                        DllPath = plugin.PluginLocation.DllPath,
                                        DllHash = currentHash,
                                        IsPresent = true
                                    }
                                };

                                plugin.PluginLocation.IsPresent = false;
                                plugin.PluginLocation = null;
                                pluginsToAdd.Add(newPlugin);
                            }
                            else
                            {
                                plugin.PluginLocation.IsPresent = true;
                            }
                        }
                        else
                        {
                            // Plugin dll is missing
                            plugin.PluginLocation.IsPresent = false;
                            plugin.PluginLocation = null;
                        }
                    }


                    _dispatcherService.Invoke(() => { plugin.NativeInstrumentsResource.Load(plugin); });
                }
            }, true, cancellationToken);

            foreach (var plugin in pluginsToAdd)
            {
                _dispatcherService.Invoke(() => { plugins.Add(plugin); });
            }

            await TaskHelper.Run(() =>
            {
                foreach (var dllPath in vstPluginDLLFiles)
                {
                    var foundPlugin = (from plugin in plugins
                            where plugin.PluginLocation != null && plugin.PluginLocation.DllPath == dllPath
                            select plugin)
                        .FirstOrDefault();

                    if (foundPlugin == null)
                    {
                        var hash = remoteFileService.GetHash(dllPath);
                        var pluginLocation = _databaseService.Context.GetPluginLocation(dllPath, hash);

                        if (pluginLocation != null)
                        {
                            pluginLocation.IsPresent = true;
                            // Got a previously missing plugin locaton. Re-assign it to the plugin
                            var lostPlugin = (from plugin in plugins
                                    where plugin.PluginLocation == null && plugin.PluginId == pluginLocation.PluginId &&
                                          plugin.HasMetadata
                                    select plugin)
                                .FirstOrDefault();

                            if (lostPlugin != null)
                            {
                                LogTo.Debug(
                                    $"Automatically connecting {pluginLocation.DllPath} to plugin {lostPlugin.PluginName} because the plugin has lost its dll, but could match it via the metadata");
                                lostPlugin.PluginLocation = pluginLocation;
                                continue;
                            }

                            var lostPluginByLastKnownGoodPath = (from plugin in plugins
                                    where plugin.LastKnownGoodDllPath == dllPath
                                    select plugin)
                                .FirstOrDefault();

                            if (lostPluginByLastKnownGoodPath != null)
                            {
                                lostPluginByLastKnownGoodPath.PluginLocation = pluginLocation;
                                continue;
                            }
                        }

                        // All failed, create new plugin
                        var newPlugin = new Plugin
                            {
                                PluginLocation = new PluginLocation
                                {
                                    DllPath = dllPath, DllHash = hash, IsPresent = true
                                }
                            };
                        _dispatcherService.Invoke(() => {plugins.Add(newPlugin);});
                        
                            
                        
                    }
                }
            }, true);


            var crashedPlugin = (from plugin in plugins where plugin.IsAnalyzing select plugin).FirstOrDefault();

            if (crashedPlugin != null)
            {
                var result = await _messageService.ShowAsync(
                    $"It seems the plugin {crashedPlugin.DllFilename} caused PresetMagician to crash during the last scan." +
                    Environment.NewLine +
                    "Would you like to disable the plugin?",
                    "Plugin crash detected", MessageButton.YesNo);

                if (result == MessageResult.Yes)
                {
                    crashedPlugin.IsEnabled = false;
                }

                crashedPlugin.IsAnalyzing = false;
            }

            await _vstService.SavePlugins();

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