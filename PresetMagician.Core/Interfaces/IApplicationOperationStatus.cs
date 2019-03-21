using System.Threading;

namespace PresetMagician.Core.Interfaces
{
    public interface IApplicationOperationStatus
    {
        bool IsApplicationBusy { get; }
        int ApplicationBusyCurrentItem { get; }
        int ApplicationBusyTotalItems { get; }
        CancellationTokenSource ApplicationBusyCancellationTokenSource { get; }
        int ApplicationBusyPercent { get; }
        string ApplicationBusyStatusText { get; }
        string ApplicationBusyOperationDescription { get; }
        bool ApplicationOperationCancelRequested { get; }
    }
}