using Catel;
using Catel.Data;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using Orchestra.Services;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.Workers;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PresetMagicianShell.ViewModels
{
    public class VstPluginViewModel : ViewModelBase
    {
        private IStatusService _statusService;
        private IPleaseWaitService _pleaseWaitService;
        private VstPluginScannerWorker _vstPluginScannerWorker;
        private VstHost _vstHost;
        private readonly IRuntimeConfigurationService _runtimeConfigurationService;
        private readonly ILog _logger = LogManager.GetCurrentClassLogger();

        public ObservableCollection<Plugin> VstPlugins { get; set; } = new ObservableCollection<Plugin>();


        public override string Title { get; protected set; } = "VST Plugins";

        public VstPluginViewModel(IStatusService statusService, IPleaseWaitService pleaseWaitService,
            IRuntimeConfigurationService runtimeConfigurationService, IServiceLocator serviceLocator)
        {
            Argument.IsNotNull(() => statusService);
            Argument.IsNotNull(() => pleaseWaitService);
            Argument.IsNotNull(() => runtimeConfigurationService);

            _pleaseWaitService = pleaseWaitService;
            _vstHost = new VstHost();
            _vstPluginScannerWorker = new VstPluginScannerWorker(_vstHost);
            _statusService = statusService;
            _runtimeConfigurationService = runtimeConfigurationService;

            serviceLocator.RegisterInstance(this);

            ScanPlugins = new TaskCommand(OnScanPluginsExecute);
            RefreshPluginList = new TaskCommand(OnRefreshPluginListExecute);
            EnablePlugin = new Command<object>(OnEnablePluginExecute);
            DisablePlugin = new Command<object>(OnDisablePluginExecute);

            RefreshPluginList.Execute();
        }

        public Command<object> EnablePlugin { get; set; }

        private void OnEnablePluginExecute(object parameter)
        {
            var plugins = (parameter as IList).Cast<Plugin>();
            
            foreach (var plugin in plugins)
            {
                plugin.Enabled = true;
            }
        }

        public Command<object> DisablePlugin { get; set; }

        private void OnDisablePluginExecute(object parameter)
        {
            var plugins = (parameter as IList).Cast<Plugin>();

            foreach (var plugin in plugins)
            {
                plugin.Enabled = false;
            }
        }

        public TaskCommand RefreshPluginList { get; set; }

        private async Task OnRefreshPluginListExecute()
        {
            ObservableCollection<String> vstPluginDLLs = new ObservableCollection<String>();
            ObservableCollection<VSTPlugin> vstPlugins = new ObservableCollection<VSTPlugin>();

            VstPlugins.Clear();
            await TaskHelper.Run(() =>
            {
                foreach (var i in _runtimeConfigurationService.RuntimeConfiguration.VstDirectories)
                {
                    foreach (string path in _vstHost.EnumeratePlugins(i.Path))
                    {
                        vstPluginDLLs.Add(path);
                    }
                }
            }, true);

            foreach (String j in vstPluginDLLs)
            {
                VstPlugins.Add(new Plugin()
                {
                    VstPlugin = new VSTPlugin(j),
                    VstPresetParser = new NullPresetParser()
                });
            }
        }

        public TaskCommand ScanPlugins { get; set; }

        private async Task OnScanPluginsExecute()
        {
            ObservableCollection<Plugin> newList = new ObservableCollection<Plugin>();

            foreach (Plugin vst in VstPlugins)
            {
                newList.Add(vst);
            };

            await TaskHelper.Run(() =>
            {
                foreach (Plugin vst in newList)
                {
                    try
                    {
                        UpdateStatus(newList.IndexOf(vst), newList.Count, String.Format("Loading {0}", vst.VstPlugin.PluginDLLPath));
                        _vstHost.LoadVST(vst.VstPlugin);
                        vst.VstPresetParser = VendorPresetParser.GetPresetHandler(vst.VstPlugin);

                        UpdateStatus(newList.IndexOf(vst), newList.Count, String.Format("Scanning banks for {0}", vst.VstPlugin.PluginDLLPath));
                        vst.VstPresetParser.ScanBanks();

                        UpdateStatus(newList.IndexOf(vst), newList.Count, String.Format("Unloading {0}", vst.VstPlugin.PluginDLLPath));
                        _vstHost.UnloadVST(vst.VstPlugin);
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        foreach (var i in e.LoaderExceptions)
                        {
                            _logger.Info(i.Message);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Info("Unable to load {0}, exception occurred: {1}", vst.VstPlugin.PluginDLLPath, e.ToString());
                    }



                }
            }, true);
        }

        private void UpdateStatus (int currentItem, int totalItems, string statusText)
        {
            _logger.Info(statusText);
            _pleaseWaitService.UpdateStatus(currentItem, totalItems, "{0} {1} " + statusText);
            _statusService.UpdateStatus("({1} / {2}) {0}", statusText, currentItem, totalItems);
        }

    }
}