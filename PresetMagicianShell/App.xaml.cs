using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Catel.IoC;
using Catel.IO;
using Catel.Logging;
using Catel.Services;
using NBug;
using NBug.Events;
using Orchestra.Services;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.Views;
using Win32Mapi;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace PresetMagicianShell
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            SetupExceptionHandling();
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += Handler.UnhandledException;
            Current.DispatcherUnhandledException += Handler.DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += Handler.UnobservedTaskException;

            NBug.Settings.CustomSubmissionEvent += Settings_CustomSubmissionEvent;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var languageService = ServiceLocator.Default.ResolveType<ILanguageService>();
            languageService.PreferredCulture = new CultureInfo("en-US");
            languageService.FallbackCulture = new CultureInfo("en-US");

#if DEBUG
            LogManager.AddDebugListener(true);
#endif

            var fileLogListener = new FileLogListener
            {
                IgnoreCatelLogging = true,
                FilePath = @"{AppDataLocal}\Logs\PresetMagician.log",
                TimeDisplay = TimeDisplay.DateTime
            };

            LogManager.AddListener(fileLogListener);
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

            
            NBug.Settings.AdditionalReportFiles.Add(fileLogListener.FilePath);


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

            var firstLogEntries = System.Text.Encoding.UTF8.GetString(result);

            

            if (firstLogEntries.Length >= readlength)
            {
                // Probably good log file, try to extract the Probably corrupted file. 

                var separatorPosition = firstLogEntries.IndexOf("=>", StringComparison.Ordinal);

                if (separatorPosition == 24)
                {
                    // Separator position found, probably valid time entry
                    var dateTimeEntry = firstLogEntries.Substring(0, separatorPosition - 1).Replace(":","-");

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
            var shellService = serviceLocator.ResolveType<IShellService>();

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
            serviceLocator.ResolveType<IRuntimeConfigurationService>().Save(true);
            base.OnExit(e);
        }

        // Custom Submission Event handler
        private void Settings_CustomSubmissionEvent(object sender, CustomSubmissionEventArgs e)
        {
            var tempZip = Path.GetTempPath() + Guid.NewGuid() + ".zip";
            var fs = new FileStream(tempZip, FileMode.Create);
            e.File.Seek(0, SeekOrigin.Begin);
            e.File.CopyTo(fs);
            fs.Close();

            var mapi = new SimpleMapi();

            mapi.AddRecipient(Settings.Links.SupportEmailName, Settings.Links.SupportEmail, false);

            mapi.Attach(tempZip);

            var myDict = new Dictionary<string, string>
            {
                ["User Comments"] = e.Report.GeneralInfo.UserDescription,
                ["Exception Type"] = e.Report.GeneralInfo.ExceptionType,
                ["Exception Backtrace"] = e.Exception.StackTrace,
                ["Application"] = e.Report.GeneralInfo.HostApplication + " " +
                                  e.Report.GeneralInfo.HostApplicationVersion
            };

            var noteText = string.Join("<br/>",
                myDict.Select(x =>
                    "<b>" + x.Key + "</b><br/><pre><code class=\"language-csharp\">" + HttpUtility.HtmlEncode(x.Value) +
                    "</code></pre>").ToArray());
            if (!mapi.Send("PresetMagician Crash: " + e.Exception.Message, noteText))
            {
                var crashesDir = Catel.IO.Path.Combine(Catel.IO.Path.GetApplicationDataDirectory(ApplicationDataTarget.UserRoaming),
                    @"Crashes\", Guid.NewGuid().ToString());

                Directory.CreateDirectory(crashesDir);
                File.Copy(tempZip, Path.Combine(crashesDir, "DiagnosticData.zip"));

                File.WriteAllText (Path.Combine(crashesDir, "ErrorDescription.txt"),noteText);

                File.WriteAllText(Path.Combine(crashesDir, "0 Send all files in this directory to.txt"), "");
                File.WriteAllText(Path.Combine(crashesDir, $"1 {Settings.Links.SupportEmail}.txt"), "");

                MessageBox.Show(
                    "Sorry, there seems to be no E-Mail client available. The Windows explorer will now open." +
                    $"Please attach the files in the opened directory and send them to {Settings.Links.SupportEmail}");

                Process.Start(crashesDir);
            }

            e.Result = true;
        }
    }
}