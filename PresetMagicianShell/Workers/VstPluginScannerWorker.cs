using Drachenkatze.PresetMagician.VendorPresetParser;
using Drachenkatze.PresetMagician.VSTHost.VST;
using PresetMagicianShell.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresetMagicianShell.Workers
{
    class VstPluginScannerWorker: BackgroundWorker
    {
        private VstHost _vstHost;

        public VstPluginScannerWorker (VstHost vstHost) : base()
        {
            WorkerReportsProgress = true;

            _vstHost = vstHost;

            DoWork += vstScanner_Worker;
            RunWorkerCompleted += vstScanner_Completed;
        }

       
        private void vstScanner_Worker(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            ObservableCollection<Plugin> vsts = (ObservableCollection<Plugin>)e.Argument;

            foreach (Plugin vst in vsts)
            {
                worker.ReportProgress((int)((100f * vsts.IndexOf(vst)) / vsts.Count), new ProgressUserState()
                {
                    StatusText = vst.VstPlugin.PluginDLLPath,
                    CurrentItem = vsts.IndexOf(vst),
                    TotalItems = vsts.Count
                });

                _vstHost.LoadVST(vst.VstPlugin);
                vst.VstPresetParser = VendorPresetParser.GetPresetHandler(vst.VstPlugin);
                vst.VstPresetParser.ScanBanks();
                _vstHost.UnloadVST(vst.VstPlugin);
            }
        }

        private void vstScanner_Completed(
            object sender, RunWorkerCompletedEventArgs e)
        {
            DoWork -= vstScanner_Worker;
            RunWorkerCompleted -= vstScanner_Completed;
           
        }
    }
}
