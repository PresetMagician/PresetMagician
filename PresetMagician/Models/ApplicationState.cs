using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Catel.Data;
using Portable.Licensing;

namespace PresetMagician.Models
{
    public class ApplicationState : ModelBase
    {
        private Type _currentDocument;

        public Type CurrentDocument
        {
            get { return _currentDocument; }
            set
            {
                if (value == typeof(PresetMagician.Views.PresetExportListView))
                {
                    SelectedRibbonTabIndex = 1;
                }

                if (value == typeof(PresetMagician.Views.VstPluginsView))
                {
                    SelectedRibbonTabIndex = 0;
                }

                RaisePropertyChanged("SelectedRibbonTabIndex");
                _currentDocument = value;
            }
        }

        #region Toolbars

        public int SelectedRibbonTabIndex { get; set; } = 1;

        #endregion

        #region ApplicationBusy

        public bool IsApplicationBusy { get; set; }
        public int ApplicationBusyCurrentItem { get; set; }
        public int ApplicationBusyTotalItems { get; set; }
        public CancellationTokenSource ApplicationBusyCancellationTokenSource { get; set; }

        public int ApplicationBusyPercent
        {
            get { return (int) (ApplicationBusyCurrentItem / (float) ApplicationBusyTotalItems * 100); }
        }

        public string ApplicationBusyStatusText { get; set; }
        public string ApplicationBusyOperationDescription { get; set; }
        public object ApplicationOperationSourceObject { get; set; }
        public string ApplicationOperationStatePropertyName { get; set; }

        public List<string> ApplicationOperationLastErrors { get; set; }
        public string ApplicationOperationLastErrorsAsText { get; set; }
        public bool ApplicationOperationLastOperationHadErrors { get; set; }
        public string ApplicationOperationLastOperation { get; set; }
        public bool ApplicationOperationCancelRequested { get; set; }

        #endregion

        #region VSTHost Workers

        public int TotalWorkers { get; set; }
        public int RunningWorkers { get; set; }

        #endregion

        public long DatabaseSize { get; set; }

        #region LicenseInformation

        public string LicensedTo
        {
            get
            {
                if (ActiveLicense != null)
                {
                    if (ActiveLicense.Type == LicenseType.Trial)
                    {
                        return $"Licensed to: {ActiveLicense.Customer.Name} " +
                               $"(Expires {ActiveLicense.Expiration.ToShortDateString()})";
                    }

                    return "Licensed to: " + ActiveLicense.Customer.Name;
                }

                return "Not Licensed";
            }
        }

        public string LicenseDescription
        {
            get
            {
                var sb = new StringBuilder();
                
                if (ActiveLicense != null)
                {
                    

                    if (ActiveLicense.Type == LicenseType.Trial)
                    {
                        sb.AppendLine($"License Type: Trial (Expires {ActiveLicense.Expiration.ToShortDateString()})");

                        if (PresetExportLimit > 0)
                        {
                            sb.AppendLine($"Maximum preset exports: {PresetExportLimit.ToString()}");
                        }
                    }
                    else
                    {
                        sb.AppendLine($"License Type: Full");
                    }
                }

                return sb.ToString();
            }
        }

        public int PresetExportLimit { get; set; }
        public string SystemCode { get; set; }
        public License ActiveLicense { get; set; }
        public bool ValidLicense { get; set; }

        #endregion
    }
}