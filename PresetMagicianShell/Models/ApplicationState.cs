using System.Collections.Generic;
using System.Threading;
using Catel.Data;

namespace PresetMagicianShell.Models
{
    public class ApplicationState: ModelBase
    {
        private bool _isPluginsRefreshing = false;
        public bool IsPluginRefreshPluginsRunning
        {
            get => _isPluginsRefreshing;
            set
            {
                AllowPluginScan = !value;
                AllowReportUnsupportedPlugins = !value;
                AllowModifyPresetExportList = !value;
                IsPluginListBusy = value;
                IsApplicationBusy = value;

                _isPluginsRefreshing = value;
            }
        }
        private bool _isPluginsScanning = false;

        public bool IsPluginScanPluginsRunning
        {
            get => _isPluginsScanning;
            set
            {
                    AllowPluginScan = !value;
                    AllowReportUnsupportedPlugins = !value;
                    AllowModifyPresetExportList = !value;
                    IsPluginListBusy = value;
                    IsApplicationBusy = value;

                _isPluginsScanning = value;
            }
        }

        #region Busy States
        public bool IsPluginListBusy { get; private set; }
        public bool IsPresetExportListBusy { get; private set; }
        #endregion

        #region Allowances
        public bool AllowPluginScan { get; private set; }
        public bool AllowReportUnsupportedPlugins { get; private set; }
        public bool AllowModifyPresetExportList { get; private set; }
        #endregion

        #region ApplicationBusy
        public bool IsApplicationBusy { get; set; }
        public int ApplicationBusyCurrentItem { get;set; }
        public int ApplicationBusyTotalItems { get;set; }
        public CancellationTokenSource ApplicationBusyCancellationTokenSource { get;set; }

        public int ApplicationBusyPercent
        {
            get { return (int) (ApplicationBusyCurrentItem / (float) ApplicationBusyTotalItems * 100); }
        }

        public string ApplicationBusyStatusText { get; set; }
        public string ApplicationBusyOperationDescription { get; set; }
        public object ApplicationOperationSourceObject { get;set; }
        public string ApplicationOperationStatePropertyName { get;set; }

        public List<string> ApplicationOperationLastErrors { get;set; }
        public string ApplicationOperationLastErrorsAsText { get;set; }
        public bool ApplicationOperationLastOperationHadErrors { get;set; }
        public string ApplicationOperationLastOperation { get;set; }
        public bool ApplicationOperationCancelRequested { get; set; }
        #endregion


    }
}