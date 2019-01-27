using System.Collections.Generic;
using System.Threading;
using Catel.MVVM;

namespace PresetMagician.Services.Interfaces
{
    public interface IApplicationService
    {
        void StartApplicationOperation(object o, string operationDescription, int totalItems);
        void CancelApplicationOperation();
        void AddApplicationOperationError(string errorMessage);
        void UpdateApplicationOperationStatus(int currentItem, string statusText);
        void StopApplicationOperation(string finalMessage);
        CancellationTokenSource GetApplicationOperationCancellationSource();

        void StartApplicationOperation(CommandContainerBase commandContainer, string operationDescription,
            int totalItems);

        List<string> GetApplicationOperationErrors();
        void ClearLastOperationErrors();
        void ReportStatus(string statusText);
        void StartProcessPool();
        void ShutdownProcessPool();
    }
}