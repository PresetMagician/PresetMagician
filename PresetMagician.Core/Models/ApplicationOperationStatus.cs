using System.ComponentModel;
using System.Threading;
using Catel.Data;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.Core.Models
{
    public class ApplicationOperationStatus : INotifyPropertyChanged, IApplicationOperationStatus
    {
        public bool IsApplicationBusy { get; set; }
        public int ApplicationBusyCurrentItem { get; set; }
        public int ApplicationBusyTotalItems { get; set; }
        public CancellationTokenSource ApplicationBusyCancellationTokenSource { get; set; }

        public int ApplicationBusyPercent =>
            (int) (ApplicationBusyCurrentItem / (float) ApplicationBusyTotalItems * 100);

        public string ApplicationBusyStatusText { get; set; }
        public string ApplicationBusyOperationDescription { get; set; }
        public bool ApplicationOperationCancelRequested { get; set; }

        public virtual event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName, object before, object after)
        {
            var x = new AdvancedPropertyChangedEventArgs(this, propertyName, before, after);
            PropertyChanged?.Invoke(this, x);
        }
    }
}