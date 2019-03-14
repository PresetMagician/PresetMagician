﻿using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Catel.IoC;
using Catel.Logging;
using Catel.Services;
using Drachenkatze.PresetMagician.Utils;
using Orc.Squirrel;
using Orchestra;
using PresetMagician.Services;
using PresetMagician.Services.Interfaces;
using PresetMagician.ViewModels;
using PresetMagician.Views;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Services;
using PresetMagician.Utils.IssueReport;
using Squirrel;
using MessageBox = System.Windows.MessageBox;

namespace PresetMagician
{
#warning TODO: FIXED: changed flag only respects preset data and not metadata. needs to be fixed.
#warning TODO: research if it's possible to work on a copy. Also ensure that while presets being edited no other operation may run!
#warning TODO: replace developer stuff with ugly company logos
#warning TODO: implement effIdle per https://www.kvraudio.com/forum/viewtopic.php?t=349866
#warning TODO: add warning in the ribbon that preset editing is active
#warning TODO: add filters for "modified presets", types, modes, probably via quick filters? hide/show ignored and mark them somehow
#warning TODO: add icons for changed since last export, preset modified
#warning TODO: add big edit box
#warning TOOD: add global types/modes editor including display which properties are user overridden
#warning TODO: add preset functions delete selected, ignore selected
#warning TODO: cancelling preset editing clears list? HUH?!?
#warning midi note name slow -> create static lookup ? or even better: only save the note number?  with ivalueconverter?
#warning modelbasefoo: only attach change listeners when editing, or better: recalc usermodified on timer basis
    #warning refactor bug report to include the new mechanism and exclude the sqlite database
    #warning add force metadata scan function
#warning todo add preview note migration
    #warning refactor types and characteristics to allow redirects and use ceras to initialize
    #warning refactor presets to include the original data from the preset parser
    #warning refactor IsMetadataModified to be calculated from the last exported metadata
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
#if !DEBUG
            SetupExceptionHandling();
#endif
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                ReportCrash(args.ExceptionObject as Exception);
            };
            Current.DispatcherUnhandledException += (sender, args) =>
            {
                ReportCrash(args.Exception);
                args.Handled = true;
            };
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                ReportCrash(args.Exception);
                args.SetObserved();
            };
        }

        public static void ReportCrash(Exception e)
        {
            var serviceLocator = ServiceLocator.Default;

            var runtimeConfigurationService = serviceLocator.ResolveType<IRuntimeConfigurationService>();
            string email = "";
            if (runtimeConfigurationService.ApplicationState.ActiveLicense?.Customer != null &&
                !string.IsNullOrWhiteSpace(runtimeConfigurationService.ApplicationState.ActiveLicense?.Customer?.Email))
            {
                email = runtimeConfigurationService.ApplicationState.ActiveLicense.Customer.Email;
            }

            var uiVisualiserService = serviceLocator.ResolveType<IUIVisualizerService>();
            var report = new IssueReport(IssueReport.TrackerTypes.CRASH, VersionHelper.GetCurrentVersion(),
                email, FileLocations.LogFile,
                DataPersisterService.DefaultPluginStoragePath);
            report.SetException(e);

            uiVisualiserService.ShowDialogAsync<ReportIssueViewModel>(report).ConfigureAwait(false);
        }


        private static ILogListener _debugListener;

        public static void SetCatelLogging(bool enable)
        {
            if (_debugListener != null)
            {
                _debugListener.IgnoreCatelLogging = !enable;
            }
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var languageService = ServiceLocator.Default.ResolveType<ILanguageService>();
            languageService.PreferredCulture = new CultureInfo("en-US");

            languageService.FallbackCulture = new CultureInfo("en-US");

#if DEBUG
            _debugListener = LogManager.AddDebugListener(true);
#endif
            var fileLogListener = new FileLogListener
            {
                IgnoreCatelLogging = true,
                FilePath = FileLocations.LogFile,
                TimeDisplay = TimeDisplay.DateTime
            };
            LogManager.IgnoreDuplicateExceptionLogging = false;
            LogManager.AddListener(fileLogListener);
            LogManager.GetCurrentClassLogger().Debug($"Started with command line {Environment.CommandLine}");

            var serviceLocator = ServiceLocator.Default;
            var updateService = serviceLocator.ResolveType<IUpdateService>();
            updateService.Initialize(Settings.Application.AutomaticUpdates.AvailableChannels,
                Settings.Application.AutomaticUpdates.DefaultChannel,
                Settings.Application.AutomaticUpdates.CheckForUpdatesDefaultValue);

            if (updateService.IsUpdateSystemAvailable)
            {
                LogManager.GetCurrentClassLogger().Debug("Update system available, processing squirrel events");
                using (var mgr = new UpdateManager(updateService.CurrentChannel.DefaultUrl))
                {
                    // Note, in most of these scenarios, the app exits after this method
                    // completes!
                    SquirrelAwareApp.HandleEvents(
                        onInitialInstall: v =>
                        {
                            LogManager.GetCurrentClassLogger().Debug("Installing shortcuts");
                            mgr.CreateShortcutForThisExe();
                            Environment.Exit(0);
                        },
                        onAppUpdate: v =>
                        {
                            LogManager.GetCurrentClassLogger().Debug("Update: Installing shortcuts");
                            mgr.CreateShortcutForThisExe();
                            Environment.Exit(0);
                        },
                        onAppUninstall: v =>
                        {
                            mgr.RemoveShortcutForThisExe();
                            Environment.Exit(0);
                        });
                }
            }


            LogManager.GetCurrentClassLogger().Debug("Startup");

            try
            {
                RotateLogFile(fileLogListener.FilePath);
            }
            catch (Exception exception)
            {
                LogManager.GetCurrentClassLogger().Error("Tried to rotate the log file, but it failed.");
                LogManager.GetCurrentClassLogger().Error(exception);
                MessageBox.Show(
                    $"Unable to rotate the log file {fileLogListener.FilePath}. Please verify that you have access to that file. " +
                    $"We will continue, but no logging will be available. Additional information: {Environment.NewLine}{Environment.NewLine}{exception}",
                    "Log File Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            // Clean up old RemoteVstHost logs
            VstUtils.CleanupVstWorkerLogDirectory();

            await StartShell();
        }

        private void RotateLogFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            byte[] result;
            const int readlength = 32;

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);

            result = new byte[readlength];
            fileStream.Read(result, 0, readlength);
            fileStream.Seek(0, SeekOrigin.Begin);

            var firstLogEntries = Encoding.UTF8.GetString(result);


            if (firstLogEntries.Length >= readlength)
            {
                // Probably good log file, try to extract the Probably corrupted file. 

                var separatorPosition = firstLogEntries.IndexOf("=>", StringComparison.Ordinal);

                if (separatorPosition == 24)
                {
                    // Separator position found, probably valid time entry
                    var dateTimeEntry = firstLogEntries.Substring(0, separatorPosition - 1).Replace(":", "-");

                    var trimmedFilePath = filePath.Replace(".log", " ");
                    trimmedFilePath += dateTimeEntry + ".log";

                    var newLogFile = new FileStream(trimmedFilePath, FileMode.Create);
                    fileStream.CopyTo(newLogFile);
                    newLogFile.Close();
                }
            }

            fileStream.Flush();
            fileStream.SetLength(0);
        }

        private async Task StartShell()
        {
            var serviceLocator = ServiceLocator.Default;
            var shellService = serviceLocator.ResolveType<CustomShellService>();

            var x = await shellService.CreateAsync<ShellWindow>();


#if DEBUG

            var ScreenNumber = 2;
            if (Screen.AllScreens.Length >= ScreenNumber)
            {
                x.WindowState = WindowState.Normal;
                var screenBounds = Screen.AllScreens[ScreenNumber - 1].Bounds;
                x.WindowStartupLocation = WindowStartupLocation.Manual;
                x.Left = screenBounds.Left;
                x.Top = screenBounds.Top;
                x.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                x.WindowState = WindowState.Maximized;
            }
#endif
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var serviceLocator = ServiceLocator.Default;
            serviceLocator.ResolveType<IRuntimeConfigurationService>().Save();
            serviceLocator.ResolveType<IApplicationService>().ShutdownProcessPool();
            base.OnExit(e);
        }
    }
}