using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CannedBytes.Midi.Message;
using Drachenkatze.PresetMagician.Controls.Controls.VSTHost;
using Drachenkatze.PresetMagician.VSTHost.VST;

namespace Drachenkatze.PresetMagician.GUI.Controls
{
    /// <summary>
    /// Interaction logic for VSTPresetListControl.xaml
    /// </summary>
    public partial class VSTPresetListControl : UserControl
    {
        private System.ComponentModel.BackgroundWorker vstPresetExporter;

        private VstHost vstHost;

        public VSTPresetListControl()
        {
            this.vstPresetExporter = new System.ComponentModel.BackgroundWorker();
            this.vstPresetExporter.WorkerReportsProgress = true;
            vstHost = new VstHost();
            InitializeComponent();
            this.DataContext = App.vstPresets;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            ExportButton.IsEnabled = false;
            Debug.WriteLine("ExportButton.click");
            vstPresetExporter.DoWork += new DoWorkEventHandler(this.presetExporter_Worker);

            vstPresetExporter.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                this.presetExporter_Completed);

            vstPresetExporter.ProgressChanged +=
                new ProgressChangedEventHandler(
            presetExporter_ProgressChanged);

            var queryResults = from preset in App.vstPresets.VstPresets
                               where preset.Export == true
                               select preset;

            foreach (var queryResult in queryResults)
            {
                Debug.WriteLine(queryResult.PresetName);
            }
            ObservableCollection<VSTPreset> presets = new ObservableCollection<VSTPreset>(queryResults);

            vstPresetExporter.RunWorkerAsync(argument: presets);
        }

        private void VSTPresetList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VSTPresetList.SelectedItems.Count == 0)
            {
                UnmarkSelectedForExport.IsEnabled = false;
                MarkSelectedForExport.IsEnabled = false;
            }
            else
            {
                UnmarkSelectedForExport.IsEnabled = true;
                MarkSelectedForExport.IsEnabled = true;
            }
        }

        private void VSTPresetList_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            DataGrid grid = (DataGrid)sender;

            if (e.Column.Header.ToString() == "Key" && e.EditingElement is TextBox && e.EditAction.HasFlag(DataGridEditAction.Commit))
            {
                TextBox t = (TextBox)e.EditingElement;
                MidiNoteName n = new MidiNoteName();

                try
                {
                    n.FullNoteName = t.Text;
                    n.NoteNumber = n.NoteNumber;

                    t.Text = n.FullNoteName;
                }
                catch (Exception)
                {
                    grid.CancelEdit();
                }
            }
        }

        private void MarkSelectedForExport_Click(object sender, RoutedEventArgs e)
        {
            if (VSTPresetList.SelectedItems.Count == 0)
            {
                return;
            }

            foreach (VSTPreset v in VSTPresetList.SelectedItems)
            {
                v.Export = true;
            }
        }

        private void UnmarkSelectedForExport_Click(object sender, RoutedEventArgs e)
        {
            if (VSTPresetList.SelectedItems.Count == 0)
            {
                return;
            }

            foreach (VSTPreset v in VSTPresetList.SelectedItems)
            {
                v.Export = false;
            }
        }

        private void presetExporter_Worker(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            ObservableCollection<VSTPreset> presets = (ObservableCollection<VSTPreset>)e.Argument;

            long currentPreset = 0;

            foreach (VSTPreset preset in presets)
            {
                currentPreset++;

                using (VSTPlugin vstPlugin = new VSTPlugin(preset.PluginDLLPath))
                {
                    vstHost.LoadVST(vstPlugin);
                    vstHost.pluginExporter.ExportPresetAudioPreviewRealtime(vstPlugin, preset);
                    vstHost.pluginExporter.ExportPresetNKSF(vstPlugin, preset);
                    vstHost.UnloadVST(vstPlugin);
                }
                worker.ReportProgress((int)((100f * currentPreset) / presets.Count), preset.PluginName + " " + preset.PresetName);
            }
        }

        private void presetExporter_Completed(
            object sender, RunWorkerCompletedEventArgs e)
        {
            vstPresetExporter.DoWork -= new DoWorkEventHandler(this.presetExporter_Worker);
            vstPresetExporter.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(
                this.presetExporter_Completed);

            vstPresetExporter.ProgressChanged -=
                new ProgressChangedEventHandler(
            presetExporter_ProgressChanged);
            App.setStatusBar("Presets exported.Remember to re-scan your user libraries in Maschine or Kontakt!");
            ExportButton.IsEnabled = true;
        }

        private void presetExporter_ProgressChanged(object sender,
            ProgressChangedEventArgs e)
        {
            App.setStatusBar("(" + e.ProgressPercentage.ToString() + "%) Exporting Preset " + e.UserState);
        }
    }
}