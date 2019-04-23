using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.Utils.Progress;

namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class PluginRemoveSelectedPluginsCommandContainer: ApplicationNotBusyCommandContainer
    {
        private readonly IAdvancedMessageService _messageService;
        private readonly DataPersisterService _dataPersister;
        private readonly PresetDataPersisterService _presetDataPersister;
        private readonly IDispatcherService _dispatcherService;
        private readonly GlobalService _globalService;
        
        public PluginRemoveSelectedPluginsCommandContainer(ICommandManager commandManager,
            IServiceLocator serviceLocator)
            : base(Commands.Plugin.RemoveSelectedPlugins, commandManager, serviceLocator)
        {
            _messageService = serviceLocator.ResolveType<IAdvancedMessageService>();
            _dataPersister = serviceLocator.ResolveType<DataPersisterService>();
            _presetDataPersister = serviceLocator.ResolveType<PresetDataPersisterService>();
            _dispatcherService = serviceLocator.ResolveType<IDispatcherService>();
            _globalService = serviceLocator.ResolveType<GlobalService>();
            _globalFrontendService.SelectedPlugins.CollectionChanged += SelectedPluginsOnCollectionChanged;
        }

        private void SelectedPluginsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            InvalidateCommand();
        }

        protected override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) &&
                   _globalFrontendService.SelectedPlugins.Count > 0;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var result = await _messageService.ShowAsync(
                "This action will remove the selected plugin(s) from the database. Note: If the plugins are still in "+
                "the configured VST directories, they will be analyzed again unless you disable them." +
                Environment.NewLine + Environment.NewLine +
                "This will also remove any imported presets for these plugins."+
                Environment.NewLine + Environment.NewLine +
                "Do you wish to continue?",
                "Remove Plugins: Please confirm", MessageButton.YesNo, MessageImage.Question);

            if (result == MessageResult.Yes)
            {
                var pluginsToRemove = _globalFrontendService.SelectedPlugins.ToList();
                
                ApplicationService.StartApplicationOperation(this, "Removing plugins",
                    pluginsToRemove.Count);
                var progress = ApplicationService.GetApplicationProgress();
                var progressStatus = new CountProgress(pluginsToRemove.Count);
                
                
                    foreach (var plugin in pluginsToRemove)
                    {
                        progressStatus.Current = pluginsToRemove.IndexOf(plugin);
                        progressStatus.Status = $"Removing {plugin.PluginName} and associated presets. This may take up to several minutes.";
                        progress.Progress.Report(progressStatus);
                        await _dispatcherService.InvokeAsync(async () => { _globalService.Plugins.Remove(plugin); });
                        _dataPersister.DeletePresetsForPlugin(plugin);
                        await _presetDataPersister.DeletePresetDataForPlugin(plugin, true);
                    }

                    progressStatus.Status = $"Compacting database. This may take up to several minutes.";
                    progress.Progress.Report(progressStatus);
                    await _presetDataPersister.VacuumDatabase();
                    
                _dataPersister.Save();
                
                ApplicationService.StopApplicationOperation("Finished removing plugins");
            }
        }
    }
}