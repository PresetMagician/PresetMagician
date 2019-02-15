using System.Diagnostics;
using System.Threading.Tasks;
using Catel.Configuration;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Drachenkatze.PresetMagician.Utils.IssueReport;
using Orchestra;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;
using SharedModels;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class HelpCreateFeatureRequestCommandContainer : CommandContainerBase
    {
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IRuntimeConfigurationService _configurationService;

        public HelpCreateFeatureRequestCommandContainer(ICommandManager commandManager,
            IUIVisualizerService uiVisualizerService, IRuntimeConfigurationService configurationService)
            : base(Commands.Help.CreateFeatureRequest, commandManager)
        {
            _uiVisualizerService = uiVisualizerService;
            _configurationService = configurationService;
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var report = new IssueReport(IssueReport.TrackerTypes.FEATURE, VersionHelper.GetCurrentVersion(),
                _configurationService.ApplicationState.ActiveLicense.Customer.Email, FileLocations.LogFile,
                ApplicationDatabaseContext.DefaultDatabasePath);

            await _uiVisualizerService.ShowDialogAsync<ReportIssueViewModel>(report);
        }
    }
}