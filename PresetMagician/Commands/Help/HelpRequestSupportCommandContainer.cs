using System.Diagnostics;
using System.Threading.Tasks;
using Catel.Configuration;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Orchestra;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Services;
using PresetMagician.Utils.IssueReport;

// ReSharper disable once CheckNamespace
namespace PresetMagician
{
    // ReSharper disable once UnusedMember.Global
    public class HelpRequestSupportCommandContainer : CommandContainerBase
    {
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IRuntimeConfigurationService _configurationService;
        private readonly GlobalService _globalService;
        
        public HelpRequestSupportCommandContainer(ICommandManager commandManager,
            IUIVisualizerService uiVisualizerService, IRuntimeConfigurationService configurationService, GlobalService globalService)
            : base(Commands.Help.RequestSupport, commandManager)
        {
            _uiVisualizerService = uiVisualizerService;
            _configurationService = configurationService;
            _globalService = globalService;
            
        }

        protected override async Task ExecuteAsync(object parameter)
        {
            var report = new IssueReport(IssueReport.TrackerTypes.SUPPORT, _globalService.PresetMagicianVersion,
                _configurationService.ApplicationState.ActiveLicense.Customer.Email, FileLocations.LogFile,
                DataPersisterService.DefaultPluginStoragePath);

            await _uiVisualizerService.ShowDialogAsync<ReportIssueViewModel>(report);
        }
    }
}