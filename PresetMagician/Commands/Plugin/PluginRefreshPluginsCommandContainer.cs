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
using SharedModels.Models;

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


        protected override async Task ExecuteThreaded(object parameter)
        {
            var vstDirectories = (from vstDirectory in _runtimeConfigurationService.RuntimeConfiguration.VstDirectories
                where vstDirectory.Active
                select vstDirectory).ToList();

            _applicationService.StartApplicationOperation(this, "Scanning VST directories for plugins",
                vstDirectories.Count);

            var cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;
            try
            {
                 var vstPluginDLLFiles =
                    await TaskHelper.Run(() => GetPluginDlLs(vstDirectories, cancellationToken), true, cancellationToken);

                var plugins = _vstService.Plugins;
                _applicationService.StopApplicationOperation("VST directory scan completed.");

                if (!cancellationToken.IsCancellationRequested)
                {
                    _applicationService.StartApplicationOperation(this, "Verifying plugins",
                        plugins.Count);
                    cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;
                    var pluginsToAdd =
                        await TaskHelper.Run(() => VerifyPlugins(plugins, cancellationToken), false, cancellationToken);

                    foreach (var plugin in pluginsToAdd)
                    {
                        _dispatcherService.Invoke(() => plugins.Add(plugin));
                    }

                    _applicationService.StopApplicationOperation("Verifying plugins complete");
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    _applicationService.StartApplicationOperation(this, "Adding/Merging plugin DLLs", vstPluginDLLFiles.Count);
                    cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;

                    await TaskHelper.Run(() => { MergeOrCreatePlugins(vstPluginDLLFiles, plugins); }, false, cancellationToken);
                }

                await _vstService.SavePlugins();
            }
            catch (Exception e)
            {
                _applicationService.AddApplicationOperationError("An unexpected error occured: "+e.Message);
                LogTo.Debug(e.StackTrace);
            }

            _applicationService.StopApplicationOperation(cancellationToken.IsCancellationRequested
                ? "VST directory scan aborted."
                : "VST directory scan completed.");
        }

        /// <summary>
        /// Creates plugins out of plugin DLL files. Checks if the DLL belongs to a plugin which has lost
        /// its previous dll plugin location (e.g. the user moved the dll away, but now moved it in again)
        /// </summary>
        /// <param name="vstPluginDLLFiles"></param>
        /// <param name="plugins"></param>
        private void MergeOrCreatePlugins(IList<string> vstPluginDLLFiles, ICollection<Plugin> plugins)
        {
            var remoteFileService = _vstService.GetVstService();
            foreach (var dllPath in vstPluginDLLFiles)
            {
                _applicationService.UpdateApplicationOperationStatus(
                    vstPluginDLLFiles.IndexOf(dllPath),
                    $"Adding/merging {dllPath}");
                
                var foundPlugin = (from plugin in plugins
                        where plugin.PluginLocation != null && plugin.PluginLocation.DllPath == dllPath
                        select plugin)
                    .FirstOrDefault();

                if (foundPlugin != null)
                {
                    continue;
                }

                var hash = remoteFileService.GetHash(dllPath);

                var pluginLocation = _databaseService.Context.GetPluginLocation(dllPath, hash);

                if (pluginLocation != null)
                {
                    pluginLocation.IsPresent = true;
                    // Got a previously missing plugin location. Find any plugin which doesn't have a pluginLocation,
                    // where the plugin IDs are matching and the plugin has metadata
                    var lostPlugin = (from plugin in plugins
                            where plugin.PluginLocation == null && plugin.VstPluginId == pluginLocation.VstPluginId &&
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
                    
                    var anyMatchingPlugin = (from plugin in plugins
                            where plugin.VstPluginId == pluginLocation.VstPluginId &&
                                  plugin.HasMetadata
                            select plugin)
                        .FirstOrDefault();

                    if (anyMatchingPlugin != null)
                    {
                        // Ignore this DLL since it's plugin ID already matches an existing plugin
                        continue;
                    }

                    // Try to match via last known good path
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

                // No previous plugin record was found, create new plugin
                var newPlugin = new Plugin
                {
                    PluginLocation = new PluginLocation
                    {
                        DllPath = dllPath, DllHash = hash, LastModifiedDateTime = remoteFileService.GetLastModifiedDate(dllPath), IsPresent = true
                    }
                };
                _dispatcherService.Invoke(() => { plugins.Add(newPlugin); });
                
            }
        }

        /// <summary>
        /// Checks all plugins in the list if they are present and if their DLL hashes have changes
        /// </summary>
        /// <param name="plugins"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of newly created plugins which were created because their DLL hash has changed</returns>
        private List<Plugin> VerifyPlugins(IList<Plugin> plugins, CancellationToken cancellationToken)
        {
            var pluginsToAdd = new List<Plugin>();
            var remoteFileService = _vstService.GetVstService();

            foreach (var plugin in plugins)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return pluginsToAdd;
                }

                if (plugin.PluginLocation != null)
                {
                    _applicationService.UpdateApplicationOperationStatus(
                        plugins.IndexOf(plugin),
                        $"Verifying {plugin.PluginLocation.DllPath}");

                    if (remoteFileService.Exists(plugin.PluginLocation.DllPath))
                    {
                        var lastModification = remoteFileService.GetLastModifiedDate(plugin.PluginLocation.DllPath);

                        if (lastModification != plugin.PluginLocation.LastModifiedDateTime)
                        {
                            var currentHash = remoteFileService.GetHash(plugin.PluginLocation.DllPath);

                            if (plugin.PluginLocation.DllHash != currentHash)
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
                                        LastModifiedDateTime = lastModification,
                                        DllHash = currentHash,
                                        IsPresent = true
                                    }
                                };

                                plugin.PluginLocation.IsPresent = false;
                                plugin.PluginLocation = null;
                                pluginsToAdd.Add(newPlugin);
                            } else {
                                plugin.PluginLocation.IsPresent = true;
                            }
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

                _dispatcherService.BeginInvoke(() => { plugin.NativeInstrumentsResource.Load(plugin); });
                Thread.Sleep(10);
            }

            return pluginsToAdd;
        }

        private List<string> GetPluginDlLs(IList<VstDirectory> vstDirectories, CancellationToken cancellationToken)
        {
            var vstPluginDLLFiles = new List<string>();

            var dllFiles = new List<string>();

            foreach (var i in vstDirectories)
            {
                _applicationService.UpdateApplicationOperationStatus(
                    vstDirectories.IndexOf(i),
                    $"Scanning {i.Path}");

                if (cancellationToken.IsCancellationRequested)
                {
                    return vstPluginDLLFiles;
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

            return vstPluginDLLFiles;
        }
    }
}