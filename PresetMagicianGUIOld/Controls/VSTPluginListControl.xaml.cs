using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
            VSTPluginList.IsEnabled = false;

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
                App.vstPlugins.VstPlugins.Add(new VSTPlugin(i));
            }

            vstPluginScanner.DoWork += vstScanner_Worker;
            vstPluginScanner.RunWorkerCompleted += vstScanner_Completed;
            vstPluginScanner.ProgressChanged += vstScanner_ProgressChanged;

            ObservableCollection<VSTPlugin> newList = new ObservableCollection<VSTPlugin>();

            foreach (VSTPlugin vst in App.vstPlugins.VstPlugins)
            {
                newList.Add(vst);
            };

            vstPluginScanner.RunWorkerAsync(argument: newList);
        }

        private void vstScanner_Worker(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            ObservableCollection<VSTPlugin> vsts = (ObservableCollection<VSTPlugin>)e.Argument;
            var VSTHost = App.vstHost;

            foreach (VSTPlugin vst in vsts)
            {
                worker.ReportProgress((int)((100f * vsts.IndexOf(vst)) / vsts.Count), vst.PluginDLLPath);

                VSTHost.LoadVST(vst);
                VSTHost.UnloadVST(vst);
            }
        }

        private void vstScanner_Completed(
            object sender, RunWorkerCompletedEventArgs e)
        {
            App.setStatusBar("VSTPlugin scan completed.");

            vstPluginScanner.DoWork -= vstScanner_Worker;
            vstPluginScanner.RunWorkerCompleted -= vstScanner_Completed;
            vstPluginScanner.ProgressChanged -= vstScanner_ProgressChanged;

            VSTPluginList.IsEnabled = true;
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
            }
            else
            {
                foreach (VSTPlugin i in VSTPluginList.SelectedItems)
                {
                    if (!i.IsLoaded)
                    {
                        CreatePresetsButton.IsEnabled = false;
                        return;
                    }
                }

                CreatePresetsButton.IsEnabled = true;
            }
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

            ObservableCollection<VSTPlugin> vsts = new ObservableCollection<VSTPlugin>();

            foreach (VSTPlugin v in VSTPluginList.SelectedItems)
            {
                vsts.Add(v);
            }
            vstPresetScanner.RunWorkerAsync(argument: vsts);

            App.activateTab(2);
        }

        private void presetScanner_Worker(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            ObservableCollection<VSTPlugin> vsts = (ObservableCollection<VSTPlugin>)e.Argument;

            scannedPresets = new ObservableCollection<VSTPreset>();
            long totalPresets = 0;
            long currentPreset = 0;
            VSTPreset preset;

            foreach (VSTPlugin plugin in vsts)
            {
                totalPresets += plugin.NumPresets;
            }

            foreach (VSTPlugin plugin in vsts)
            {
                for (int i = 0; i < plugin.NumPresets; i++)
                {
                    currentPreset++;
                    preset = plugin.getPreset(i);
                    worker.ReportProgress((int)((100f * currentPreset) / totalPresets), preset.VstPlugin.PluginName + " " + preset.PresetName);
                    scannedPresets.Add(preset);
                }
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
    }
}