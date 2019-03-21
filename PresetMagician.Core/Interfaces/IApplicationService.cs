using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Catel.MVVM;
using PresetMagician.Core.ApplicationTask;
using PresetMagician.Core.Models;

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

        List<string> GetApplicationOperationErrors();
        void ClearLastOperationErrors();
        void ReportStatus(string statusText);
        void StartProcessPool();
        void ShutdownProcessPool();
        void SetApplicationOperationTotalItems(int items);
        ApplicationProgress GetApplicationProgress();
        ApplicationOperationStatus GetApplicationOperationStatus();
        void Initialize();

    }
}