using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Anotar.Catel;
using Catel;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Catel.Windows;
using Drachenkatze.PresetMagician.VendorPresetParser;
using PresetMagician.Models.EventArgs;
using PresetMagician.ProcessIsolation;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using SharedModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public abstract class AbstractScanPluginsCommandContainer : ApplicationNotBusyCommandContainer
    {
        protected readonly IVstService _vstService;
        protected readonly IApplicationService _applicationService;
        protected readonly IDispatcherService _dispatcherService;
        protected readonly ICommandManager _commandManager;
        protected readonly IDatabaseService _databaseService;
        protected readonly INativeInstrumentsResourceGeneratorService _resourceGeneratorService;
        private readonly IAdvancedMessageService _messageService;
        private int _totalPresets;
        private int _currentPresetIndex;
        private int updateThrottle;
        private int _currentPluginIndex;
        private Plugin _currentPlugin;

        protected AbstractScanPluginsCommandContainer(string command, ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService, IVstService vstService,
            IApplicationService applicationService,
            IDispatcherService dispatcherService, IAdvancedMessageService messageService,
            IDatabaseService databaseService, INativeInstrumentsResourceGeneratorService resourceGeneratorService)
            : base(command, commandManager, runtimeConfigurationService)
        {
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => dispatcherService);
            Argument.IsNotNull(() => databaseService);
            Argument.IsNotNull(() => resourceGeneratorService);
            Argument.IsNotNull(() => messageService);

            _messageService = messageService;
            _vstService = vstService;
            _applicationService = applicationService;
            _dispatcherService = dispatcherService;
            _databaseService = databaseService;
            _commandManager = commandManager;
            _resourceGeneratorService = resourceGeneratorService;

            _vstService.Plugins.CollectionChanged += OnPluginsListChanged;
        }

        protected abstract List<Plugin> GetPluginsToScan();


        protected virtual bool IsQuickAnalysisMode()
        {
            return false;
        }
        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && _vstService.Plugins.Count > 0;
        }

        protected void OnPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var pluginsToScan = GetPluginsToScan();


            var pluginsToUpdate =
                (from p in _vstService.Plugins where (!p.IsAnalyzed || !p.IsPresent) && p.IsEnabled orderby p.PluginName, p.DllFilename select p).ToList();

            _applicationService.StartApplicationOperation(this, "Analyzing VST plugins: Loading missing metadata",
                pluginsToUpdate.Count);
            var cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;

            var pluginsToRemove = new List<Plugin>();
            // First pass: Load missing metadata
            try
            {
                
                pluginsToRemove = await TaskHelper.Run(async () => await UpdateMetadata(pluginsToUpdate, cancellationToken), true,
                        cancellationToken);

                _applicationService.StopApplicationOperation("Analyzing VST plugins Metadata analysis complete.");

                await _dispatcherService.InvokeAsync(() =>
                {
                    foreach (var plugin in pluginsToRemove)
                    {
                        _vstService.Plugins.Remove(plugin);

                        if (pluginsToScan.Contains(plugin))
                        {
                            pluginsToScan.Remove(plugin);
                        }
                    }
                });
            }
            catch (Exception e)
            {
                _applicationService.AddApplicationOperationError(
                    $"Unable to update metadata because of {e.Message}");
                LogTo.Debug(e.StackTrace);
            }

            if (pluginsToRemove.Count > 0)
            {
                var pluginNames = string.Join(Environment.NewLine,
                    (from plugin in pluginsToRemove orderby plugin.PluginName select plugin.PluginName).Distinct()
                    .ToList());


                var result = await _messageService.ShowAsync(
                    "Automatically merged different plugin DLLs to the same plugin. Affected plugin(s):" +
                    Environment.NewLine + Environment.NewLine +
                    pluginNames + Environment.NewLine + Environment.NewLine +
                    "Would you like to abort the analysis now, so that you can review the settings for each affected plugin? (Highly recommended!)",
                    "Auto-merged Plugins", HelpLinks.SETTINGS_PLUGIN_DLL, MessageButton.YesNo, MessageImage.Question);

                if (result == MessageResult.Yes)
                {
                    // ReSharper disable once MethodSupportsCancellation
                    await _databaseService.Context.SaveChangesAsync();
                    _commandManager.ExecuteCommand(Commands.Application.CancelOperation);
                }
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                _applicationService.StartApplicationOperation(this, "Analyzing VST plugins",
                    pluginsToScan.Count);
                cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;

                _databaseService.Context.PresetUpdated += ContextOnPresetUpdated;
                await TaskHelper.Run(async () => await AnalyzePlugins(pluginsToScan, cancellationToken), true,
                    cancellationToken);

                // ReSharper disable once MethodSupportsCancellation
                await _databaseService.Context.SaveChangesAsync();
                _databaseService.Context.PresetUpdated -= ContextOnPresetUpdated;
            }


            if (cancellationToken.IsCancellationRequested)
            {
                _applicationService.StopApplicationOperation("Plugin analysis cancelled.");
            }
            else
            {
                _applicationService.StopApplicationOperation("Plugin analysis completed.");
            }

            var unreportedPlugins =
                (from plugin in _vstService.Plugins
                    where !plugin.IsReported && !plugin.DontReport && !plugin.IsSupported && plugin.IsAnalyzed && plugin.IsEnabled
                    select plugin).ToList();

            if (unreportedPlugins.Any())
            {
                var result = await _messageService.ShowAsyncWithDontAskAgain("There are unsupported plugins which are not reported." +
                                                             Environment.NewLine +
                                                             "Would you like to report them, so we can implement support for them?",
                    "Report Unsupported Plugins", null, MessageButton.YesNo, MessageImage.Question, "Don't ask again for the currently unreported plugins");


                if (result.result == MessageResult.Yes)
                {
                    _commandManager.ExecuteCommand(Commands.Plugin.ReportUnsupportedPlugins);
                }

                if (result.dontAskAgainChecked)
                {
                    foreach (var plugin in unreportedPlugins)
                    {
                        plugin.DontReport = true;
                    }
                    await _databaseService.Context.SaveChangesAsync();
                }
            }
        }

        private void ContextOnPresetUpdated(object sender, PresetUpdatedEventArgs e)
        {
            updateThrottle++;
            _currentPresetIndex++;

            if (_currentPresetIndex > _totalPresets)
            {
                Debug.WriteLine(
                    $"{e.NewValue.Plugin.PluginName}: Got called with {e.NewValue.PresetName} index {_currentPresetIndex} of {_totalPresets}");
            }

            if (updateThrottle > 10)
            {
                _applicationService.UpdateApplicationOperationStatus(
                    _currentPluginIndex,
                    $"Adding/Updating presets for {_currentPlugin.PluginName} ({_currentPresetIndex} / {_totalPresets}): Preset {e.NewValue.PresetName}");
                updateThrottle = 0;
            }
        }

        private void PresetsOnCollectionChanged(object sender, EventArgs e)
        {
            updateThrottle++;

            if (updateThrottle <= 10)
            {
                return;
            }

            var collection = sender as ObservableCollection<Preset>;
            _applicationService.UpdateApplicationOperationStatus(
                _currentPluginIndex,
                $"Pre-loading presets from database for {_currentPlugin.PluginName}: Preset #{collection.Count}");
            updateThrottle = 0;
        }

        private async Task AnalyzePlugins(IList<Plugin> pluginsToScan, CancellationToken cancellationToken)
        {
            foreach (var plugin in pluginsToScan)
            {
                if (!plugin.IsPresent)
                {
                    continue;
                }

                try
                {
                    using (var remotePluginInstance = _vstService.GetRemotePluginInstance(plugin))
                    {
                        _vstService.SelectedPlugin = plugin;
                        _applicationService.UpdateApplicationOperationStatus(
                            pluginsToScan.IndexOf(plugin), $"Scanning {plugin.DllFilename}");

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        plugin.IsAnalyzing = true;

                        _databaseService.Context.CompressPresetData =
                            _runtimeConfigurationService.RuntimeConfiguration.CompressPresetData;
                        _databaseService.Context.PreviewNote =
                            _runtimeConfigurationService.RuntimeConfiguration.DefaultPreviewMidiNote;

                        if (!plugin.HasMetadata)
                        {
                            if (plugin.LoadError)
                            {
                                LogTo.Debug($"Skipping {plugin.DllPath} because a load error occured");
                            }
                            else
                            {
                                throw new Exception(
                                    $"Plugin {plugin.DllPath} has no metadata and was loaded correctly.");
                            }
                        }

                        plugin.Debug($"Attempting to find presetParser for {plugin.PluginName}");

                        VendorPresetParser.DeterminatePresetParser(remotePluginInstance);
                        var wasLoaded = remotePluginInstance.IsLoaded;
                        plugin.PresetParser.DataPersistence = _databaseService.GetPresetDataStorer();

                        if (plugin.PresetParser.SupportsAdditionalBankFiles)
                        {
                            plugin.PresetParser.AdditionalBankFiles.Clear();
                            plugin.PresetParser.AdditionalBankFiles.AddRange(plugin.AdditionalBankFiles);
                        }

                        _currentPluginIndex = pluginsToScan.IndexOf(plugin);
                        _currentPlugin = plugin;

                        plugin.Presets.CollectionCountChanged += PresetsOnCollectionChanged;
                        _databaseService.Context.LoadPresetsForPlugin(plugin);
                        plugin.Presets.CollectionCountChanged -= PresetsOnCollectionChanged;


                        if (!IsQuickAnalysisMode())
                        {
                            using (plugin.Presets.SuspendChangeNotifications())
                            {
                                using (plugin.Presets.SuspendChangeNotifications())
                                {
                                    plugin.PresetParser.Presets = plugin.Presets;
                                    plugin.PresetParser.RootBank = plugin.RootBank.First();

                                    _totalPresets = plugin.PresetParser.GetNumPresets();
                                    _currentPresetIndex = 0;

                                    await plugin.PresetParser.DoScan();
                                }
                            }
                        }

                        plugin.IsAnalyzed = true;

                        if (_runtimeConfigurationService.RuntimeConfiguration.AutoCreateResources &&
                            _resourceGeneratorService.ShouldCreateScreenshot(remotePluginInstance))
                        {
                            plugin.Debug(
                                $"Auto-generating resources for {plugin.DllFilename} - Opening Editor",
                                plugin.DllFilename);
                            _applicationService.UpdateApplicationOperationStatus(
                                pluginsToScan.IndexOf(plugin),
                                $"Auto-generating resources for {plugin.DllFilename} - Opening Editor");
                            if (!remotePluginInstance.IsLoaded)
                            {
                                await remotePluginInstance.LoadPlugin();
                            }

                            remotePluginInstance.OpenEditorHidden();
                            _dispatcherService.Invoke(() => Application.Current.MainWindow.BringWindowToTop());
                            await Task.Delay(1000);
                        }

                        await _dispatcherService.InvokeAsync(() =>
                        {
                            if (_runtimeConfigurationService.RuntimeConfiguration.AutoCreateResources)
                            {
                                plugin.Debug(
                                    $"Auto-generating resources for {plugin.DllFilename} - Creating screenshot and applying magic");
                                _applicationService.UpdateApplicationOperationStatus(
                                    pluginsToScan.IndexOf(plugin),
                                    $"Auto-generating resources for {plugin.DllFilename} - Creating screenshot and applying magic");

                                _resourceGeneratorService.AutoGenerateResources(remotePluginInstance);
                            }
                        });
                        wasLoaded = remotePluginInstance.IsLoaded;


                        if (!IsQuickAnalysisMode())
                        {
                            await _databaseService.Context.Flush();
                        }

                        if (wasLoaded)
                        {
                            remotePluginInstance.UnloadPlugin();
                            plugin.Debug($"Unloading {plugin.DllFilename}");
                        }
                    }
                }
                catch (Exception e)
                {
                    plugin.OnLoadError(e);
                    _applicationService.AddApplicationOperationError(
                        $"Unable to analyze {plugin.DllFilename} because of {e.Message}");
                    plugin.Debug(e.StackTrace);
                }

                plugin.IsAnalyzing = false;
                _applicationService.UpdateApplicationOperationStatus(
                    pluginsToScan.IndexOf(plugin),
                    $"{plugin.DllFilename} - Updating Database");
                _databaseService.Context.SaveChanges();
            }
        }

        private async Task<List<Plugin>> UpdateMetadata(IList<Plugin> pluginsToUpdate,
            CancellationToken cancellationToken)
        {
            var vstService = _vstService.GetVstService();
            var pluginsToRemove = new List<Plugin>();

            foreach (var plugin in pluginsToUpdate)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return pluginsToRemove;
                }

                _vstService.SelectedPlugin = plugin;
                _applicationService.UpdateApplicationOperationStatus(
                    pluginsToUpdate.IndexOf(plugin),
                    $"Loading metadata for {plugin.DllFilename}");

                if (!plugin.HasMetadata)
                {
                    if (plugin.PluginLocation == null)
                    {
                        // No metadata and no plugin location dll is not present => remove it
                        if (!_databaseService.Context.HasPresets(plugin))
                        {
                            LogTo.Info(
                                $"Removing unknown plugin entry without metadata, without presets and without plugin dll");

                            pluginsToRemove.Add(plugin);
                        }

                        continue;
                    }

                    if (!vstService.Exists(plugin.PluginLocation.DllPath))
                    {
                        plugin.PluginLocation.IsPresent = false;
                        plugin.PluginLocation = null;
                        continue;
                    }

                    using (var remotePluginInstance = _vstService.GetRemotePluginInstance(plugin))
                    {
                        await remotePluginInstance.LoadPlugin();
                        remotePluginInstance.UnloadPlugin();
                    }
                }

                if (!plugin.HasMetadata)
                {
                    // Still no metadata, probably plugin loading failure
                    continue;
                }

                // We now got metadata - check if there's an existing plugin with the same plugin ID. If so,
                // merge this plugin with the existing one if it has no presets.
                var existingPlugin =
                    (from p in _vstService.Plugins where p.VstPluginId == plugin.VstPluginId && p != plugin select p)
                    .FirstOrDefault();

                if (existingPlugin != null && !_databaseService.Context.HasPresets(plugin))
                {
                    // There's an existing plugin which this plugin can be merged into. Schedule it for removal
                    pluginsToRemove.Add(plugin);

                    // If the existing plugin is not present, but this one is: Move over the plugin location
                    if (!existingPlugin.IsPresent)
                    {
                        await _dispatcherService.InvokeAsync(() =>
                        {
                            existingPlugin.PluginLocation = plugin.PluginLocation;
                        });
                    }
                }

                await _dispatcherService.InvokeAsync(() => { plugin.NativeInstrumentsResource.Load(plugin); });
            }

            return pluginsToRemove;
        }
    }
}