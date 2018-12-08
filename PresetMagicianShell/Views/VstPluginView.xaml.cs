using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Catel.IoC;
using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagicianShell.Models;
using PresetMagicianShell.Services.Interfaces;

namespace PresetMagicianShell.Views
{
    /// <summary>
    /// Interaction logic for VSTPluginListControl.xaml
    /// </summary>
    public partial class VstPluginView 
    {
        private System.ComponentModel.BackgroundWorker vstPluginScanner;
        private System.ComponentModel.BackgroundWorker vstPresetScanner;
        private ObservableCollection<VSTPreset> scannedPresets;

        public VstHost VstHost { get; set; }

        public VstPluginView()
        {
            InitializeComponent();
            this.vstPluginScanner = new System.ComponentModel.BackgroundWorker();
            this.vstPluginScanner.WorkerReportsProgress = true;

            this.vstPresetScanner = new System.ComponentModel.BackgroundWorker();
            this.vstPresetScanner.WorkerReportsProgress = true;
            VstHost = new VstHost();
        }

        private void ScanPluginButton_Click(object sender, RoutedEventArgs e)
        {
            ScanPluginButton.IsEnabled = false;
            ReportUnsupportedPlugins.IsEnabled = false;

            

            
            //vstPluginScanner.ProgressChanged += vstScanner_ProgressChanged;

            ObservableCollection<Plugin> newList = new ObservableCollection<Plugin>();

            /*foreach (Plugin vst in VstPlugins)
            {
                newList.Add(vst);
            };*/

            vstPluginScanner.RunWorkerAsync(argument: newList);
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
           /* if (VSTPluginList.SelectedItems.Count == 0)
            {
                return;
            }

            vstPresetScanner.DoWork += this.presetScanner_Worker;

            vstPresetScanner.RunWorkerCompleted += presetScanner_Completed;

            vstPresetScanner.ProgressChanged += presetScanner_ProgressChanged;

            ObservableCollection<Plugin> vsts = new ObservableCollection<Plugin>();

            vsts.Clear();

            foreach (Plugin v in VSTPluginList.SelectedItems)
            {
                vsts.Add(v);
            }
            vstPresetScanner.RunWorkerAsync(argument: vsts);

            App.activateTab(2);*/
        }

        private void presetScanner_Worker(object sender, DoWorkEventArgs e)
        {
            /*var VSTHost = App.vstHost;
            
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
            }*/
        }

        private void presetScanner_Completed(
            object sender, RunWorkerCompletedEventArgs e)
        {
            /*foreach (VSTPreset p in scannedPresets)
            {
                App.vstPresets.VstPresets.Add(p);
            }

            vstPresetScanner.DoWork -= this.presetScanner_Worker;

            vstPresetScanner.RunWorkerCompleted -= presetScanner_Completed;

            vstPresetScanner.ProgressChanged -= presetScanner_ProgressChanged;
            App.setStatusBar("Presets prepared.");*/
        }

        private void presetScanner_ProgressChanged(object sender,
            ProgressChangedEventArgs e)
        {
            //App.setStatusBar("(" + e.ProgressPercentage.ToString() + "%) Preparing Preset " + e.UserState);
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

            /*App.vstHost.LoadVST(v.VstPlugin);
            var items = v.VstPlugin.getPluginInfo();
            var pluginInfoWindow = new PluginInfoWindow();
            pluginInfoWindow.PluginProperties.ItemsSource = items;
            pluginInfoWindow.ShowDialog();
            App.vstHost.UnloadVST(v.VstPlugin);*/
        }

        private async void ReportUnsupportedPlugins_OnClick(object sender, RoutedEventArgs e)
        {
            /*ReportUnsupportedPlugins.IsEnabled = false;
            List<Plugin> pluginsToReport = new List<Plugin>();

            foreach (Plugin p in App.vstPlugins.VstPlugins)
            {
                if (p.VstPlugin.IsLoaded && !p.IsSupported)
                {
                    pluginsToReport.Add(p);
                }
            }

            var response = await App.submitPlugins(pluginsToReport);

            MessageBox.Show(response);

            ReportUnsupportedPlugins.IsEnabled = true;*/
        }
    }
}