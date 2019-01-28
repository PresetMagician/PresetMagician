using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Anotar.Catel;
using Catel;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Catel.Windows;
using Drachenkatze.PresetMagician.VendorPresetParser;
using PresetMagician.Extensions;
using PresetMagician.Models;
using PresetMagician.Models.EventArgs;
using PresetMagician.ProcessIsolation;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using SharedModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public abstract class AbstractScanPluginsCommandContainer : CommandContainerBase
    {
        protected readonly IRuntimeConfigurationService _runtimeConfigurationService;
        protected readonly IVstService _vstService;
        protected readonly IApplicationService _applicationService;
        protected readonly IDispatcherService _dispatcherService;
        protected readonly ICommandManager _commandManager;
        protected readonly IDatabaseService _databaseService;
        protected readonly INativeInstrumentsResourceGeneratorService _resourceGeneratorService;
        private int _totalPresets;
        private int _currentPresetIndex;
        private int updateThrottle;
        private int _currentPluginIndex;
        private Plugin _currentPlugin;

        protected AbstractScanPluginsCommandContainer(string command, ICommandManager commandManager,
            IRuntimeConfigurationService runtimeConfigurationService, IVstService vstService,
            IApplicationService applicationService,
            IDispatcherService dispatcherService,
            IDatabaseService databaseService, INativeInstrumentsResourceGeneratorService resourceGeneratorService)
            : base(command, commandManager)
        {
            Argument.IsNotNull(() => runtimeConfigurationService);
            Argument.IsNotNull(() => vstService);
            Argument.IsNotNull(() => applicationService);
            Argument.IsNotNull(() => dispatcherService);
            Argument.IsNotNull(() => databaseService);
            Argument.IsNotNull(() => resourceGeneratorService);

            _runtimeConfigurationService = runtimeConfigurationService;
            _vstService = vstService;
            _applicationService = applicationService;
            _dispatcherService = dispatcherService;
            _databaseService = databaseService;
            _commandManager = commandManager;
            _resourceGeneratorService = resourceGeneratorService;

            _vstService.Plugins.CollectionChanged += OnPluginsListChanged;
            _runtimeConfigurationService.ApplicationState.PropertyChanged += OnAllowPluginScanChanged;
        }

        protected abstract List<Plugin> GetPluginsToScan();


        protected override bool CanExecute(object parameter)
        {
            return _vstService.Plugins.Count > 0 &&
                   _runtimeConfigurationService.ApplicationState.AllowPluginScan;
        }

        protected void OnAllowPluginScanChanged(object o, PropertyChangedEventArgs ev)
        {
            if (ev.PropertyName == nameof(ApplicationState.AllowPluginScan)) InvalidateCommand();
        }

        protected void OnPluginsListChanged(object o, NotifyCollectionChangedEventArgs ev)
        {
            InvalidateCommand();
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var cancellationToken = _applicationService.GetApplicationOperationCancellationSource().Token;

            var pluginsToRemove = new List<Plugin>();

            var pluginsToUpdate =
                (from p in _vstService.Plugins where (!p.IsAnalyzed || !p.IsPresent) && p.IsEnabled select p).ToList();

            _applicationService.StartApplicationOperation(this, "Analyzing VST plugins: Loading missing metadata",
                pluginsToUpdate.Count);
            // First pass: Load missing metadata
            await TaskHelper.Run(async () =>
            {
                foreach (var plugin in pluginsToUpdate)
                {
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

                        var remoteFileService = ProcessPool.GetRemoteFileService().Result;
                        if (!remoteFileService.Exists(plugin.PluginLocation.DllPath))
                        {
                            plugin.PluginLocation.IsPresent = false;
                            plugin.PluginLocation = null;
                            continue;
                        }

                        var remotePluginInstance = await _vstService.GetRemotePluginInstance(plugin);

                        await remotePluginInstance.LoadPlugin();
                        remotePluginInstance.KillHost();
                    }

                    if (plugin.HasMetadata)
                    {
                        var existingPlugin =
                            (from p in _vstService.Plugins where p.PluginId == plugin.PluginId && p != plugin select p)
                            .FirstOrDefault();

                        if (existingPlugin != null)
                        {
                            if (!_databaseService.Context.HasPresets(plugin))
                            {
                                if (!existingPlugin.IsPresent)
                                {
                                    await _dispatcherService.InvokeAsync(() =>
                                    {
                                        existingPlugin.PluginLocation = plugin.PluginLocation;
                                    });
                                }

                                pluginsToRemove.Add(plugin);
                            }
                        }

                        await _dispatcherService.InvokeAsync(() => { plugin.NativeInstrumentsResource.Load(plugin); });
                    }
                }
            }, true);

            _applicationService.StopApplicationOperation("Analyzing VST plugins Metadata analysis complete.");
            await _dispatcherService.InvokeAsync(() =>
            {
                foreach (var plugin in pluginsToRemove)
                {
                    _vstService.Plugins.Remove(plugin);
                }
            });

            var pluginsToScan = GetPluginsToScan();

            _applicationService.StartApplicationOperation(this, "Analyzing VST plugins",
                pluginsToScan.Count);

            _databaseService.Context.PresetUpdated += ContextOnPresetUpdated;
            await TaskHelper.Run(async () =>
            {
                IRemotePluginInstance remotePluginInstance = null;

                foreach (var plugin in pluginsToScan)
                {
                    var wasLoaded = false;
                    if (!plugin.IsPresent)
                    {
                        continue;
                    }

                    try
                    {
                        _vstService.SelectedPlugin = plugin;
                        _applicationService.UpdateApplicationOperationStatus(
                            pluginsToScan.IndexOf(plugin),
                            $"Scanning {plugin.DllFilename}");

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

                        remotePluginInstance = await _vstService.GetRemotePluginInstance(plugin);

                        plugin.Debug($"Attempting to find presetParser for {plugin.PluginName}");

                        VendorPresetParser.DeterminatePresetParser(remotePluginInstance);
                        wasLoaded = remotePluginInstance.IsLoaded;
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
                        wasLoaded = remotePluginInstance.IsLoaded;
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


                        await _databaseService.Context.Flush();
                        if (wasLoaded)
                        {
                            remotePluginInstance.UnloadPlugin();
                            plugin.Debug($"Unloading {plugin.DllFilename}");
                        }
                    }
                    catch (Exception e)
                    {
                        plugin.OnLoadError(e);
                        _applicationService.AddApplicationOperationError(
                            $"Unable to analyze {plugin.DllFilename} because of {e.Message}");
                        plugin.Debug(e.StackTrace);
                    }

                    if (remotePluginInstance != null)
                    {
                        remotePluginInstance.KillHost();
                    }

                    plugin.IsAnalyzing = false;
                    _databaseService.Context.SaveChanges();
                }
            }, true);

            await _databaseService.Context.SaveChangesAsync();
            _databaseService.Context.PresetUpdated -= ContextOnPresetUpdated;

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
                    where !plugin.IsReported && !plugin.IsSupported && plugin.IsAnalyzed && plugin.IsEnabled
                    select plugin).Any();

            if (unreportedPlugins)
            {
                var result =
                    MessageBox.Show(
                        "There are unsupported plugins which are not reported." + Environment.NewLine +
                        "Would you like to report them, so we can implement support for them?",
                        "Report Unsupported Plugins", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    _commandManager.ExecuteCommand(Commands.Plugin.ReportUnsupportedPlugins);
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
    }
}