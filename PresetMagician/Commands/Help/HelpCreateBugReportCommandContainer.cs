using System.Threading.Tasks;
using Catel.MVVM;
using Catel.Services;
using PresetMagician.Core.Services;
using PresetMagician.Utils.IssueReport;
using PresetMagician.ViewModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class HelpCreateBugReportCommandContainer : CommandContainerBase
    {
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly GlobalService _globalService;
        private readonly GlobalFrontendService _globalFrontendService;

        public HelpCreateBugReportCommandContainer(ICommandManager commandManager,
            IUIVisualizerService uiVisualizerService, GlobalService globalService,
            GlobalFrontendService globalFrontendService)
            : base(Commands.Help.CreateBugReport, commandManager)
        {
            _uiVisualizerService = uiVisualizerService;
            _globalFrontendService = globalFrontendService;
            _globalService = globalService;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var report = new IssueReport(IssueReport.TrackerTypes.BUG, _globalService.PresetMagicianVersion,
                _globalFrontendService.ApplicationState.ActiveLicense.Customer.Email, FileLocations.LogFile,
                DataPersisterService.DefaultPluginStoragePath);

            await _uiVisualizerService.ShowDialogAsync<ReportIssueViewModel>(report);
        }
    }
}