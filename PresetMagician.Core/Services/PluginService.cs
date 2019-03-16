using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Catel.Collections;
using Catel.Services;
using Drachenkatze.PresetMagician.Utils;
using Drachenkatze.PresetMagician.Utils.Progress;
using Orchestra;
using PresetMagician.Core.ApplicationTask;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Utils.Logger;
using PresetMagician.Utils.Progress;

namespace PresetMagician.Core.Services
{
    public class PluginService
    {
        private IDispatcherService _dispatcherService;
        private IVstService _vstService;
        private Dictionary<string, string> DllHashes = new Dictionary<string, string>();

        public PluginService(IVstService vstService, IDispatcherService dispatcherService,
            DataPersisterService dataPersisterService)
        {
            _dispatcherService = dispatcherService;
            _vstService = vstService;
        }

        /// <summary>
        /// Returns all plugin DLLs found in the specified directories
        /// </summary>
        /// <param name="vstDirectories">The VST directories to scan</param>
        /// <param name="applicationProgress">The progress reporter</param>
        /// <returns></returns>
        public HashSet<string> GetPluginDlls(IList<VstDirectory> vstDirectories, ApplicationProgress applicationProgress)
        {
            var vstPluginDLLFiles = new HashSet<string>();

            var dllFiles = new List<string>();
            var directoriesToScan = (from directory in vstDirectories where directory.Active select directory).ToList();

            var progressStatus = new CountProgress(directoriesToScan.Count);

            foreach (var i in directoriesToScan)
            {
                progressStatus.Current = directoriesToScan.IndexOf(i);
                progressStatus.Status = i.Path;

                applicationProgress.Progress.Report(progressStatus);

                if (applicationProgress.CancellationToken.IsCancellationRequested)
                {
                    return vstPluginDLLFiles;
                }

                try
                {
                    dllFiles = VstUtils.EnumeratePlugins(i.Path);
                }
                catch (Exception e)
                {
                    applicationProgress.LogReporter.Report(new LogEntry(LogLevel.Error,
                        $"Unable to scan {i.Path} because of {e.Message}"));
                }

                if (dllFiles.Count > 0)
                {
                    vstPluginDLLFiles.AddRange(dllFiles);
                }
            }

            return vstPluginDLLFiles;
        }

        /// <summary>
        /// Checks all plugins in the list if they are present and if their DLL hashes have changes
        /// </summary>
        /// <param name="plugins"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>List of newly created plugins which were created because their DLL hash has changed</returns>
        public List<Plugin> VerifyPlugins(IList<Plugin> plugins, HashSet<string> dllPaths, ApplicationProgress applicationProgress)
        {
            var remoteFileService = _vstService.GetRemoteVstService();
            
            var pluginsToAdd = new List<Plugin>();
            var progressStatus = new CountProgress(plugins.Count);
            foreach (var plugin in plugins)
            {
                if (applicationProgress.CancellationToken.IsCancellationRequested)
                {
                    return pluginsToAdd;
                }

                progressStatus.Status = $"Verifying {plugin.PluginLocation.DllPath}";
                progressStatus.Current = plugins.IndexOf(plugin);
                applicationProgress.Progress.Report(progressStatus);

                UpdatePluginLocations(plugin);
                
                if (plugin.PluginLocation != null && !plugin.PluginLocation.IsPresent)
                {
                    var newPluginLocation =
                        (from p in plugin.PluginLocations where p.IsPresent select p).FirstOrDefault();
                    plugin.PluginLocation = newPluginLocation;
                }

                foreach (var pluginLocation in plugin.PluginLocations)
                {
                    if (pluginLocation.IsPresent)
                    {
                        dllPaths.Remove(pluginLocation.DllPath);
                    }
                }
            }

            foreach (var plugin in plugins)
            {
                _dispatcherService.BeginInvoke(() => { plugin.NativeInstrumentsResource.Load(plugin); });
                Thread.Sleep(10);
            }

            foreach (var dllPath in dllPaths)
            {
                var newPlugin = new Plugin
                {
                    PluginLocation = new PluginLocation
                    {
                        DllPath = dllPath, DllHash = GetDllHash(dllPath),
                        LastModifiedDateTime = remoteFileService.GetLastModifiedDate(dllPath), IsPresent = true
                    }
                };
                
                pluginsToAdd.Add(newPlugin);
            }

            return pluginsToAdd;
        }

        /// <summary>
        /// Updates all plugin locations for a plugin
        /// </summary>
        /// <param name="plugin"></param>
        private void UpdatePluginLocations(Plugin plugin)
        {
            var remoteFileService = _vstService.GetRemoteVstService();

            foreach (var pluginLocation in plugin.PluginLocations)
            {
                if (!remoteFileService.Exists(pluginLocation.DllPath))
                {
                    pluginLocation.IsPresent = false;
                    continue;
                }

                var lastModification = remoteFileService.GetLastModifiedDate(pluginLocation.DllPath);

                if (lastModification == pluginLocation.LastModifiedDateTime)
                {
                    pluginLocation.IsPresent = true;
                    continue;
                }

                var currentHash = GetDllHash(pluginLocation.DllPath);

                pluginLocation.IsPresent = pluginLocation.DllHash == currentHash;
            }
        }

        /// <summary>
        /// Calculate the DLL hashes for each plugin
        /// </summary>
        /// <param name="vstPluginDLLFiles"></param>
        /// <param name="plugins"></param>
        /// <param name="remoteFileService"></param>
        private string GetDllHash(string dllPath) {
           
            var remoteFileService = _vstService.GetRemoteVstService();

           
                var hash = remoteFileService.GetHash(dllPath);

                if (DllHashes.ContainsKey(dllPath))
                {
                    DllHashes[dllPath] = hash;
                }
                else
                {
                    DllHashes.Add(dllPath, hash);
                }

                return DllHashes[dllPath];
        }
        
        public async Task<List<Plugin>> UpdateMetadata(IList<Plugin> pluginsToUpdate,
            ApplicationProgress applicationProgress)
        {
            var pluginsToRemove = new List<Plugin>();
            var progressStatus = new CountProgress(pluginsToUpdate.Count);

            foreach (var plugin in pluginsToUpdate)
            {
                if (applicationProgress.CancellationToken.IsCancellationRequested)
                {
                    return pluginsToRemove;
                }

                try
                {
                    _vstService.SelectedPlugin = plugin;
                    progressStatus.Current = pluginsToUpdate.IndexOf(plugin);
                    progressStatus.Status = $"Loading metadata for {plugin.DllFilename}";
                    applicationProgress.Progress.Report(progressStatus);

                    if (!plugin.HasMetadata)
                    {
                        if (plugin.PluginLocation == null)
                        {
                            // No metadata and no plugin location dll is not present => remove it
                            if (plugin.Presets.Count == 0)
                            {
                                applicationProgress.LogReporter.Report(new LogEntry(LogLevel.Info, "Removing unknown plugin entry without metadata, without presets and without plugin dll"));
                                pluginsToRemove.Add(plugin);
                            }

                            continue;
                        }

                        using (var remotePluginInstance = _vstService.GetRemotePluginInstance(plugin, false))
                        {
                            await remotePluginInstance.LoadPlugin();
                            remotePluginInstance.UnloadPlugin();
                        }
                    }

                    if (!plugin.HasMetadata)
                    {
                        plugin.LastFailedAnalysisVersion = VersionHelper.GetCurrentVersion();
                        // Still no metadata, probably plugin loading failure
                        continue;
                    }

                    // We now got metadata - check if there's an existing plugin with the same plugin ID. If so,
                    // merge this plugin with the existing one if it has no presets.
                    var existingPlugin =
                        (from p in _vstService.Plugins
                            where p.VstPluginId == plugin.VstPluginId && p.PluginId != plugin.PluginId
                            select p)
                        .FirstOrDefault();

                    if (existingPlugin != null)
                    {
                        if (plugin.Presets.Count == 0)
                        {
                            // There's an existing plugin which this plugin can be merged into. Schedule it for removal
                            pluginsToRemove.Add(plugin);

                            if (existingPlugin.PluginLocation == null)
                            {
                                await _dispatcherService.InvokeAsync(() =>
                                {
                                    existingPlugin.PluginLocation = plugin.PluginLocation;
                                });
                            } else
                            {
                                existingPlugin.PluginLocations.Add(plugin.PluginLocation);
                            }
                        } else if (existingPlugin.Presets.Count == 0)
                        {
                            // The existing plugin has no presets - remove it!
                            pluginsToRemove.Add(existingPlugin);

                            if (plugin.PluginLocation == null)
                            {
                                await _dispatcherService.InvokeAsync(() =>
                                {
                                    plugin.PluginLocation = existingPlugin.PluginLocation;
                                });
                            } else
                            {
                                plugin.PluginLocations.Add(existingPlugin.PluginLocation);
                            }
                           
                        }
                    } 
                   
                }
                catch (Exception e)
                {
                    applicationProgress.LogReporter.Report(new LogEntry(LogLevel.Error, $"Unable to load  metadata for {plugin.DllFilename} because of {e.GetType().FullName} {e.Message}"));
                   
                    plugin.OnLoadError(e);

                    if (!plugin.HasMetadata)
                    {
                        plugin.LastFailedAnalysisVersion = VersionHelper.GetCurrentVersion();
                    }
                }

                await _dispatcherService.InvokeAsync(() => { plugin.NativeInstrumentsResource.Load(plugin); });
            }

            return pluginsToRemove;
        }
    }
}