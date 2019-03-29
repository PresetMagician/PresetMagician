using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using Catel;
using Catel.MVVM;
using PresetMagician.Core.Services;

namespace PresetMagician.Services
{
    public class FrontendService
    {
        private readonly ICommandManager _commandManager;
        private readonly GlobalService _globalService;
        private Dictionary<IEnumerable, object> _collectionLocks = new Dictionary<IEnumerable, object>();


        public FrontendService(ICommandManager commandManager, GlobalService globalService)
        {
            _commandManager = commandManager;
            _globalService = globalService;
        }

        public void SetupCollectionSynchronizations()
        {
            SetupCollectionSynchronization(_globalService.Plugins);
            SetupCollectionSynchronization(_globalService.RemoteVstHostProcessPool.OldProcesses);
            SetupCollectionSynchronization(_globalService.RemoteVstHostProcessPool.RunningProcesses);
        }

        public void SetupCollectionSynchronization(IEnumerable collection)
        {
            if (Application.Current != null)
            {
                // Only do this when running as WPF app
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var _lock = new object();
                    _collectionLocks.Add(collection, _lock);
                    BindingOperations.EnableCollectionSynchronization(collection, _lock);
                }));
            }
        }

        public void InitializeCommands()
        {
            _commandManager.CreateCommandWithGesture(typeof(Commands.Application), "CancelOperation");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Application), "ClearLastOperationErrors");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Application), "ApplyConfiguration");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Application), "NotImplemented");

            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "ScanPlugins");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.ScanSelectedPlugins));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.ScanSelectedPlugin));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.ForceReloadMetadata));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "RefreshPlugins");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "AllToPresetExportList");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "SelectedToPresetExportList");

            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.NotExportedAllToPresetExportList));

            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.NotExportedSelectedToPresetExportList));

            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin), "ReportUnsupportedPlugins");

            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.ForceReportPluginsToLive));

            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.ForceReportPluginsToDev));
            
            _commandManager.CreateCommandWithGesture(typeof(Commands.Plugin),
                nameof(Commands.Plugin.RemoveSelectedPlugins));

            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "EnablePlugins");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "DisablePlugins");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools),
                nameof(Commands.PluginTools.ViewSettings));

            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools),
                nameof(Commands.PluginTools.ViewPresets));

            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools),
                nameof(Commands.PluginTools.ViewErrors));
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "ShowPluginInfo");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "ShowPluginEditor");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "ShowPluginChunk");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "LoadPlugin");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools), "UnloadPlugin");
            _commandManager.CreateCommandWithGesture(typeof(Commands.PluginTools),
                nameof(Commands.PluginTools.ReportSinglePluginToLive));


            _commandManager.CreateCommandWithGesture(typeof(Commands.PresetExport),
                nameof(Commands.PresetExport.ClearSelected));
            _commandManager.CreateCommandWithGesture(typeof(Commands.PresetExport),
                nameof(Commands.PresetExport.ActivatePresetView));
            _commandManager.CreateCommandWithGesture(typeof(Commands.PresetExport),
                nameof(Commands.PresetExport.DoExport));
            _commandManager.CreateCommandWithGesture(typeof(Commands.PresetExport),
                nameof(Commands.PresetExport.ClearList));

            _commandManager.CreateCommandWithGesture(typeof(Commands.PresetTools),
                nameof(Commands.PresetTools.ShowPresetData));

            _commandManager.CreateCommandWithGesture(typeof(Commands.Tools), "NksfView");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Tools), "SettingsView");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Tools), nameof(Commands.Tools.UpdateLicense));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Tools),
                nameof(Commands.Tools.EditTypesCharacteristics));

            _commandManager.CreateCommandWithGesture(typeof(Commands.Help), nameof(Commands.Help.RequestSupport));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Help), nameof(Commands.Help.CreateBugReport));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Help), nameof(Commands.Help.CreateFeatureRequest));
            _commandManager.CreateCommandWithGesture(typeof(Commands.Help), "OpenChatLink");
            _commandManager.CreateCommandWithGesture(typeof(Commands.Help), "OpenDocumentationLink");

            _commandManager.CreateCommandWithGesture(typeof(Commands.Developer),
                nameof(Commands.Developer.SetCatelLogging));
        }
    }
}