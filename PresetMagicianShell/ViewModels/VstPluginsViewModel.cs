using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Data;
using Catel;
using Catel.Collections;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Drachenkatze.PresetMagician.VSTHost.VST;
using NuGet;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;

namespace PresetMagicianShell.ViewModels
{
    public class VstPluginsViewModel : ViewModelBase
    {
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();
        private readonly IPleaseWaitService _pleaseWaitService;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly ICustomStatusService _statusService;
        private readonly IUIVisualizerService _uiVisualizerService;

        private readonly IViewModelFactory _viewModelFactory;
        private readonly VstHost _vstHost;
        private readonly IVstService _vstService;

        public VstPluginsViewModel(ICustomStatusService statusService, IPleaseWaitService pleaseWaitService,
            IRuntimeConfigurationService runtimeConfigurationService, IServiceLocator serviceLocator,
            IViewModelFactory viewModelFactory, IUIVisualizerService uiVisualizerService, IVstService vstService, ICommandManager commandManager)
        {
            Argument.IsNotNull(() => statusService);
            Argument.IsNotNull(() => pleaseWaitService);
            Argument.IsNotNull(() => runtimeConfigurationService);
            Argument.IsNotNull(() => serviceLocator);
            Argument.IsNotNull(() => viewModelFactory);
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => vstService);
            _vstService = vstService;

            _pleaseWaitService = pleaseWaitService;
            _vstHost = new VstHost();
            _statusService = statusService;
            _runtimeConfigurationService = runtimeConfigurationService;
            _viewModelFactory = viewModelFactory;
            _uiVisualizerService = uiVisualizerService;

            Plugins = vstService.Plugins;

            serviceLocator.RegisterInstance(this);

            ScanPlugins = new TaskCommand(OnScanPluginsExecute);
            
            EnablePlugin = new Command<object>(OnEnablePluginExecute);
            DisablePlugin = new Command<object>(OnDisablePluginExecute);
            ShowPluginInfo = new Command<object>(OnShowPluginInfoExecute);

            var pView = CollectionViewSource.GetDefaultView(Plugins);

            pView.SortDescriptions.Add(new SortDescription("Enabled", ListSortDirection.Descending));
            pView.SortDescriptions.Add(new SortDescription("IsSupported", ListSortDirection.Descending));
            pView.SortDescriptions.Add(new SortDescription("PluginName", ListSortDirection.Ascending));


            var productview = (ICollectionViewLiveShaping) CollectionViewSource.GetDefaultView(pView);
            productview.IsLiveSorting = true;

            commandManager.ExecuteCommand(Commands.Plugin.RefreshPlugins);
            Title = "VST Plugins";
        }

        public Plugin SelectedPlugin
        {
            get => _vstService.SelectedPlugin;
            set => _vstService.SelectedPlugin = value;
        }


        public FastObservableCollection<Plugin> Plugins { get; }
        public ApplicationState ApplicationState { get; }

        public bool IsScanning { get; private set; }
        public int ScanProgressPercent { get; private set; }
        public string ScanProgressText { get; private set; }

        public Command<object> EnablePlugin { get; set; }

        public Command<object> DisablePlugin { get; set; }

        public Command<object> ShowPluginInfo { get; set; }

        public TaskCommand ScanPlugins { get; set; }

        private void OnEnablePluginExecute(object parameter)
        {
            var plugins = (parameter as IList).Cast<Plugin>();

            foreach (var plugin in plugins) plugin.Enabled = true;
        }

        private void OnDisablePluginExecute(object parameter)
        {
            var plugins = (parameter as IList).Cast<Plugin>();

            foreach (var plugin in plugins) plugin.Enabled = false;
        }

        private void OnShowPluginInfoExecute(object parameter)
        {
            var plugin = parameter as Plugin;

            _uiVisualizerService.ShowDialogAsync<VstPluginInfoViewModel>(plugin);
        }

        private async Task OnScanPluginsExecute()
        {
            IsScanning = true;
            var newList = (from plugin in Plugins where plugin.Enabled select plugin).ToList();

            await TaskHelper.Run(() =>
            {
                foreach (var vst in newList)
                    try
                    {
                        UpdateStatus(newList.IndexOf(vst) + 1, newList.Count, $"Loading {vst.DllPath}");
                        _vstHost.LoadVST(vst);

                        if (vst.IsLoaded)
                        {
                            vst.DeterminatePresetParser();

                            UpdateStatus(newList.IndexOf(vst) + 1, newList.Count, $"Scanning banks for {vst.DllPath}");
                            vst.PresetParser.ScanBanks();
                            vst.RootBank.PresetBanks.Clear();
                            vst.RootBank.PresetBanks.Add(vst.PresetParser.RootBank);
                            vst.NumPresets = vst.PresetParser.Presets.Count;
                            vst.Presets = vst.PresetParser.Presets;
                            vst.IsScanned = true;

                            UpdateStatus(newList.IndexOf(vst) + 1, newList.Count, $"Unloading {vst.DllPath}");
                            _vstHost.UnloadVST(vst);
                        }
                        else
                        {
                            _logger.Info("Unable to load {0}", vst.DllPath, vst.LoadErrorMessage);
                        }

                        UpdateStatus(newList.IndexOf(vst) + 1, newList.Count, $"Done scanning {vst.DllPath}");
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        foreach (var i in e.LoaderExceptions) _logger.Info(i.Message);
                    }
                    catch (Exception e)
                    {
                        _logger.Info("Unable to load {0}, exception occurred: {1}", vst.DllPath, e.ToString());
                        _logger.Info("{0}", e.StackTrace);
                    }
            }, true);

            IsScanning = false;
            _pleaseWaitService.Hide();
            CollectionViewSource.GetDefaultView(Plugins).Refresh();
        }

        private void UpdateStatus(int currentItem, int totalItems, string statusText)
        {
            var progressText = string.Format("({1} / {2}) {0}", statusText, currentItem, totalItems);
            ScanProgressPercent = (int) (currentItem / (float) totalItems * 100);
            ScanProgressText = progressText;
            _pleaseWaitService.UpdateStatus(currentItem, totalItems, "{0} {1} " + statusText);
            _statusService.UpdateStatus(progressText);
        }
    }
}