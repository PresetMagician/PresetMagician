using System.Collections.Generic;
using System.Text;
using System.Threading;
using Catel.Data;
using Catel.MVVM;
using Portable.Licensing;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.Core.Models
{
    /// <summary>
    /// Holds the global application state, including global collections like Plugins, Presets etc.
    /// </summary>
    public class ApplicationState : ModelBase, IApplicationOperationStatus
    {
        public IViewModel CurrentDocumentViewModel { get; set; }


        #region Toolbars

        public int SelectedRibbonTabIndex { get; set; } = 1;

        #endregion

        #region ApplicationBusy

        public bool IsApplicationBusy { get; private set; }

        public int ApplicationBusyCurrentItem { get; private set; }
        public int ApplicationBusyTotalItems { get; private set; }
        public CancellationTokenSource ApplicationBusyCancellationTokenSource { get; private set; }

        public int ApplicationBusyPercent { get; private set; }

        public string ApplicationBusyStatusText { get; private set; }
        public string ApplicationBusyOperationDescription { get; private set; }

        public List<string> ApplicationOperationLastErrors { get; set; }
        public string ApplicationOperationLastErrorsAsText { get; set; }
        public bool ApplicationOperationLastOperationHadErrors { get; set; }
        public string ApplicationOperationLastOperation { get; set; }
        public bool ApplicationOperationCancelRequested { get; private set; }

        public bool IsApplicationEditing { get; set; }

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

        public void ApplyFromApplicationOperationStatus(ApplicationOperationStatus applicationOperationStatus)
        {
            IsApplicationBusy = applicationOperationStatus.IsApplicationBusy;
            ApplicationBusyCurrentItem = applicationOperationStatus.ApplicationBusyCurrentItem;
            ApplicationBusyTotalItems = applicationOperationStatus.ApplicationBusyTotalItems;
            ApplicationBusyCancellationTokenSource = applicationOperationStatus.ApplicationBusyCancellationTokenSource;
            ApplicationBusyPercent = applicationOperationStatus.ApplicationBusyPercent;
            ApplicationBusyStatusText = applicationOperationStatus.ApplicationBusyStatusText;
            ApplicationBusyOperationDescription = applicationOperationStatus.ApplicationBusyOperationDescription;
            ApplicationOperationCancelRequested = applicationOperationStatus.ApplicationOperationCancelRequested;
        }
    }
}