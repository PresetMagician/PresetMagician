using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using Catel.Services;
using Drachenkatze.PresetMagician.Utils.Progress;
using PresetMagician.Core.Interfaces;
using PresetMagician.Models;
using PresetMagician.Services.Interfaces;
using PresetMagician.Core.Models;
using PresetMagician.Utils.IssueReport;

namespace PresetMagician.ViewModels
{
    public sealed class ReportIssueViewModel : ViewModelBase
    {
        private IAdvancedMessageService _advancedMessageService;

        public string BoxTitle { get; set; }

        public string ReportDescriptionHint { get; set; }
        public ObservableCollection<Plugin> Plugins { get; } = new ObservableCollection<Plugin>();
        public Plugin SelectedPlugin { get; set; }
        public bool IsSubmitting { get; private set; }
        public bool DisplayBugWarning { get; private set; }
        public string SubmitProgress { get; private set; }
        public bool DisplayIncludeSupportFiles { get; private set; } = true;

        public bool MayIncludePlugins
        {
            get
            {
                if (Report == null)
                {
                    return false;
                }
                var includePlugins = false;

                switch (Report.TrackerType)
                {
                    case IssueReport.TrackerTypes.BUG:
                    case IssueReport.TrackerTypes.FEATURE:
                        includePlugins = true;
                        break;
                }

                return includePlugins;
            }
        }

        public bool DisplayPluginLogCheckbox
        {
            get
            {
                if (MayIncludePlugins && DisplayIncludeSupportFiles)
                {
                    return true;
                }

                return false;
            }
        }

        public IssueReport Report { get; set; }

        public ReportIssueViewModel(IssueReport report, IVstService vstService,
            IAdvancedMessageService advancedMessageService)
        {
            Report = report;

            switch (report.TrackerType)
            {
                case IssueReport.TrackerTypes.BUG:
                    Title = "Report Bug";
                    BoxTitle = "Report Bug";
                    ReportDescriptionHint =
                        "Please describe how and when the bug occured, and if possible how to reproduce the bug.";
                    Report.IncludeSystemLog = true;
                    Report.SubmitPrivately = true;
                    DisplayBugWarning = true;
                    break;
                case IssueReport.TrackerTypes.FEATURE:
                    Title = "Report Feature Request";
                    BoxTitle = "Create Feature Request";
                    ReportDescriptionHint = "Please describe the feature request.";
                    DisplayIncludeSupportFiles = false;
                    break;
                case IssueReport.TrackerTypes.CRASH:
                    Title = "Fatal Error";
                    BoxTitle = "Sorry, PresetMagician encountered a fatal error :(";
                    ReportDescriptionHint =
                        "If you can, provide information about what you did before the crash occurred.";
                    Report.IncludeSystemLog = true;
                    Report.SubmitPrivately = true;
                    break;
                case IssueReport.TrackerTypes.SUPPORT:
                    Title = "General Support";
                    BoxTitle = "General Support Request";
                    Report.SubmitPrivately = true;
                    DisplayIncludeSupportFiles = false;

                    break;
            }

            if (MayIncludePlugins)
            {
                var plugin = new Plugin {PluginName = "<none>"};
                Plugins.Add(plugin);
                Plugins.AddRange(vstService.Plugins.ToList().OrderBy(p => p.PluginName));
            }

            _advancedMessageService = advancedMessageService;

            Report.PropertyChanged += ReportOnPropertyChanged;
            PropertyChanged += OnPropertyChanged;
            SubmitIssue = new ProgressiveTaskCommand<StringProgress>(OnSubmitIssueExecute, null, ReportProgress);
            CloseDialog = new TaskCommand(OnCloseDialogExecute, CanExecute);
        }

        private bool CanExecute()
        {
            return !IsSubmitting;
        }

        private void ReportProgress(StringProgress obj)
        {
            SubmitProgress = obj.Status;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedPlugin))
            {
                if (SelectedPlugin.PluginId != "")
                {
                    if (Report.TrackerType == IssueReport.TrackerTypes.BUG)
                    {
                        Report.IncludePluginLog = true;
                        Report.PluginLog = SelectedPlugin.Logs;
                        Report.IncludeDatabase = true;
                    }

                    Report.PluginId = SelectedPlugin.PluginId;
                    Report.PluginName = SelectedPlugin.PluginName;
                    Report.PluginVendor = SelectedPlugin.PluginVendor;
                    Report.PluginVstId = SelectedPlugin.VstPluginId.ToString();
                }
                else
                {
                    Report.IncludePluginLog = false;
                    Report.PluginId = null;
                }
            }
        }

        private void ReportOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IssueReport.IncludeDatabase))
            {
                if (Report.IncludeDatabase)
                {
                    Report.SubmitPrivately = true;
                }
            }

            if (e.PropertyName == nameof(IssueReport.IncludeSystemLog))
            {
                if (Report.IncludeSystemLog)
                {
                    Report.SubmitPrivately = true;
                }
            }
        }


        public ProgressiveTaskCommand<StringProgress> SubmitIssue { get; set; }

        private async Task OnSubmitIssueExecute(CancellationToken token, IProgress<StringProgress> progress)
        {
            Validate(true);
            Report.Validate(true);

            if (Report.GetValidationContextForObjectGraph().HasErrors)
            {
                return;
            }

            IsSubmitting = true;
            CloseDialog.RaiseCanExecuteChanged();
            string resultMessage = "";
            string resultTitle = "";
            var messageBoxImage = MessageImage.Question;

            try
            {
                await Report.PrepareIssue(progress);
                progress.Report(new StringProgress("Submitting issue"));
                await Report.SubmitIssue();

                resultMessage =
                    "Issue submitted successfully. You'll receive additional information about the progress of your report via E-Mail";
                resultTitle = "Issue submitted successfully";
            }
            catch (Exception e)
            {
                resultMessage = $"The issue could not be submitted. {e.GetType().FullName}: {e.Message}";
                resultTitle = "Error submitting issue";
                messageBoxImage = MessageImage.Error;
            }

            if (Report.TrackerType == IssueReport.TrackerTypes.CRASH)
            {
                await ConfirmCrashClose(resultMessage, resultTitle, messageBoxImage);
            }
            else
            {
                await _advancedMessageService.ShowAsync(
                    resultMessage,
                    resultTitle, MessageButton.OK, messageBoxImage);
            }

            IsSubmitting = false;

            await this.CancelAndCloseViewModelAsync();
        }

        protected async Task ConfirmCrashClose(string resultMessage, string resultTitle, MessageImage messageBoxImage)
        {
            resultMessage +=
                $"{Environment.NewLine}{Environment.NewLine}Would you like to close PresetMagician now (recommended)?";
            var result = await _advancedMessageService.ShowAsync(
                resultMessage, resultTitle, null, MessageButton.YesNo, messageBoxImage);

            if (result == MessageResult.No)
            {
                await _advancedMessageService.ShowWarningAsync(
                    "Not closing PresetMagician after an internal crash might cause other strange effects, bugs or even data loss. It is highly recommended to close PresetMagician now - continue at your own risk.",
                    "Warning");
            }
            else
            {
                Application.Current.Shutdown(-1);
            }
        }

        public TaskCommand CloseDialog { get; set; }


        private async Task OnCloseDialogExecute()
        {
            if (Report.TrackerType == IssueReport.TrackerTypes.CRASH)
            {
                await ConfirmCrashClose("", "Stop PresetMagician", MessageImage.Question);
            }

            await this.CancelAndCloseViewModelAsync();
        }
    }
}