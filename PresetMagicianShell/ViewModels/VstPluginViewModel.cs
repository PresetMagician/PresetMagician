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
            _statusService = statusService;
            _runtimeConfigurationService = runtimeConfigurationService;

            serviceLocator.RegisterInstance(this);

            ScanPlugins = new TaskCommand(OnScanPluginsExecute);
            RefreshPluginList = new TaskCommand(OnRefreshPluginListExecute);
            EnablePlugin = new Command<object>(OnEnablePluginExecute);
            DisablePlugin = new Command<object>(OnDisablePluginExecute);

            VstPlugins = runtimeConfigurationService.RuntimeConfiguration.Plugins;
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

            foreach (String dllPath in vstPluginDLLs)
            {
                var foundPlugin = (from plugin in VstPlugins where plugin.DllPath == dllPath select plugin).FirstOrDefault();

                if (foundPlugin == null)
                {
                    VstPlugins.Add(new Plugin()
                    {
                        DllPath = dllPath
                    });
                }
            }
        }

        public TaskCommand ScanPlugins { get; set; }

        private async Task OnScanPluginsExecute()
        {
            //ObservableCollection<Plugin> newList = new ObservableCollection<Plugin>();

            var newList = (from plugin in VstPlugins where plugin.Enabled == true select plugin).ToList();

            await TaskHelper.Run(() =>
            {
                foreach (Plugin vst in newList)
                {
                    try
                    {
                        UpdateStatus(newList.IndexOf(vst), newList.Count, $"Loading {vst.DllPath}");
                        _vstHost.LoadVST(vst);
                        vst.DeterminatePresetParser();

                        UpdateStatus(newList.IndexOf(vst), newList.Count, $"Scanning banks for {vst.DllPath}");
                        vst.PresetParser.ScanBanks();
                        vst.NumPresets = vst.PresetParser.NumPresets;

                        UpdateStatus(newList.IndexOf(vst), newList.Count, $"Unloading {vst.DllPath}");
                        _vstHost.UnloadVST(vst);
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
                        _logger.Info("Unable to load {0}, exception occurred: {1}", vst.DllPath, e.ToString());
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