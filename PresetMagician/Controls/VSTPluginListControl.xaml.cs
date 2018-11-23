using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Drachenkatze.PresetMagician.GUI.GUI;
using Drachenkatze.PresetMagician.GUI.Models;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VendorPresetParser.StandardVST;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.GUI.Controls
{
    /// <summary>
    /// Interaction logic for VSTPluginListControl.xaml
    /// </summary>
    public partial class VSTPluginListControl : UserControl
    {
        private System.ComponentModel.BackgroundWorker vstPluginScanner;
        private System.ComponentModel.BackgroundWorker vstPresetScanner;
        private ObservableCollection<VSTPreset> scannedPresets;

        public VSTPluginListControl()
        {
            InitializeComponent();
            this.vstPluginScanner = new System.ComponentModel.BackgroundWorker();
            this.vstPluginScanner.WorkerReportsProgress = true;

            this.vstPresetScanner = new System.ComponentModel.BackgroundWorker();
            this.vstPresetScanner.WorkerReportsProgress = true;

            this.DataContext = App.vstPlugins;
        }

        private void ScanPluginButton_Click(object sender, RoutedEventArgs e)
        {
            App.setStatusBar("Scanning VSTPlugin paths...");
            ScanPluginButton.IsEnabled = false;

            ObservableCollection<String> vstPluginDLLs = new ObservableCollection<String>();
            ObservableCollection<VSTPlugin> vstPlugins = new ObservableCollection<VSTPlugin>();

            App.vstPlugins.VstPlugins.Clear();

            foreach (DirectoryInfo i in App.vstPaths.VstPaths)
            {
                var VSTHost = App.vstHost;
                foreach (string path in VSTHost.EnumeratePlugins(i))
                {
                    vstPluginDLLs.Add(path);
                }
            }

            foreach (String i in vstPluginDLLs)
            {
                App.vstPlugins.VstPlugins.Add(new Plugin()
                {
                    VstPlugin = new VSTPlugin(i),
                    VstPresetParser = new NullPresetParser()
                });
            }

            vstPluginScanner.DoWork += vstScanner_Worker;
            vstPluginScanner.RunWorkerCompleted += vstScanner_Completed;
            vstPluginScanner.ProgressChanged += vstScanner_ProgressChanged;

            ObservableCollection<Plugin> newList = new ObservableCollection<Plugin>();

            foreach (Plugin vst in App.vstPlugins.VstPlugins)
            {
                newList.Add(vst);
            };

            vstPluginScanner.RunWorkerAsync(argument: newList);
        }

        private void vstScanner_Worker(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            ObservableCollection<Plugin> vsts = (ObservableCollection<Plugin>)e.Argument;
            var VSTHost = App.vstHost;

            foreach (Plugin vst in vsts)
            {
                worker.ReportProgress((int)((100f * vsts.IndexOf(vst)) / vsts.Count), vst.VstPlugin.PluginDLLPath);

                VSTHost.LoadVST(vst.VstPlugin);
                vst.VstPresetParser = VendorPresetParser.VendorPresetParser.GetPresetHandler(vst.VstPlugin);
                vst.VstPresetParser.ScanBanks();
                VSTHost.UnloadVST(vst.VstPlugin);
            }
        }

        private void vstScanner_Completed(
            object sender, RunWorkerCompletedEventArgs e)
        {
            App.setStatusBar("VSTPlugin scan completed.");

            vstPluginScanner.DoWork -= vstScanner_Worker;
            vstPluginScanner.RunWorkerCompleted -= vstScanner_Completed;
            vstPluginScanner.ProgressChanged -= vstScanner_ProgressChanged;

            ScanPluginButton.IsEnabled = true;
            VSTPluginList.Items.Refresh();
        }

        private void vstScanner_ProgressChanged(object sender,
            ProgressChangedEventArgs e)
        {
            App.setStatusBar("(" + e.ProgressPercentage.ToString() + "%) Loading " + e.UserState);
        }

        private void VSTPluginList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VSTPluginList.SelectedItems.Count == 0)
            {
                CreatePresetsButton.IsEnabled = false;
                ShowPluginInfo.IsEnabled = false;
                return;
            }

            if (VSTPluginList.SelectedItems.Count == 1)
            {
                ShowPluginInfo.IsEnabled = true;
            }

            foreach (Plugin i in VSTPluginList.SelectedItems)
            {
                if (!i.VstPlugin.IsLoaded)
                {
                    CreatePresetsButton.IsEnabled = false;
                    return;
                }
            }

            CreatePresetsButton.IsEnabled = true;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (VSTPluginList.SelectedItems.Count == 0)
            {
                return;
            }

            App.setStatusBar("Preparing presets...");

            vstPresetScanner.DoWork += this.presetScanner_Worker;

            vstPresetScanner.RunWorkerCompleted += presetScanner_Completed;

            vstPresetScanner.ProgressChanged += presetScanner_ProgressChanged;

            ObservableCollection<Plugin> vsts = new ObservableCollection<Plugin>();

            vsts.Clear();
            App.vstPresets.VstPresets.Clear();

            foreach (Plugin v in VSTPluginList.SelectedItems)
            {
                vsts.Add(v);
            }
            vstPresetScanner.RunWorkerAsync(argument: vsts);

            App.activateTab(2);
        }

        private void presetScanner_Worker(object sender, DoWorkEventArgs e)
        {
            var VSTHost = App.vstHost;
            BackgroundWorker worker = sender as BackgroundWorker;
            ObservableCollection<Plugin> vsts = (ObservableCollection<Plugin>)e.Argument;

            scannedPresets = new ObservableCollection<VSTPreset>();

            foreach (Plugin plugin in vsts)
            {
                VSTHost.LoadVST(plugin.VstPlugin);
                foreach (var bank in plugin.VstPresetParser.Banks)
                {
                    foreach (var preset in bank.VSTPresets)
                    {
                        scannedPresets.Add(preset);
                    }
                }
                VSTHost.UnloadVST(plugin.VstPlugin);
            }
        }

        private void presetScanner_Completed(
            object sender, RunWorkerCompletedEventArgs e)
        {
            foreach (VSTPreset p in scannedPresets)
            {
                App.vstPresets.VstPresets.Add(p);
            }

            vstPresetScanner.DoWork -= this.presetScanner_Worker;

            vstPresetScanner.RunWorkerCompleted -= presetScanner_Completed;

            vstPresetScanner.ProgressChanged -= presetScanner_ProgressChanged;
            App.setStatusBar("Presets prepared.");
        }

        private void presetScanner_ProgressChanged(object sender,
            ProgressChangedEventArgs e)
        {
            App.setStatusBar("(" + e.ProgressPercentage.ToString() + "%) Preparing Preset " + e.UserState);
        }

        private void VSTPluginList_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
        }

        private void ShowPluginInfo_OnClick(object sender, RoutedEventArgs e)
        {
            if (VSTPluginList.SelectedItems.Count != 1)
            {
                return;
            }

            Plugin v = (Plugin)VSTPluginList.SelectedItem;

            App.vstHost.LoadVST(v.VstPlugin);
            var items = v.VstPlugin.getPluginInfo();
            var pluginInfoWindow = new PluginInfoWindow();
            pluginInfoWindow.PluginProperties.ItemsSource = items;
            pluginInfoWindow.ShowDialog();
            App.vstHost.UnloadVST(v.VstPlugin);
        }
    }
}