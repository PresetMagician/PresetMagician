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
using Catel.Logging;
using Catel.Services;
using NBug;
using NBug.Events;
using Orchestra.Services;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.Views;
using Win32Mapi;

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

#if DEBUG
            LogManager.AddDebugListener(true);
#endif

            var fileLogListener = new FileLogListener
                {IgnoreCatelLogging = true, FilePath = @"{AppDataLocal}\{AutoLogFileName}"};

            LogManager.AddListener(fileLogListener);

            var languageService = ServiceLocator.Default.ResolveType<ILanguageService>();
            languageService.PreferredCulture = new CultureInfo("en-US");
            languageService.FallbackCulture = new CultureInfo("en-US");


            await StartShell();
        }


        private async Task StartShell()
        {
            var serviceLocator = ServiceLocator.Default;
            var shellService = serviceLocator.ResolveType<IShellService>();

            var x = await shellService.CreateAsync<ShellWindow>();


           
#if DEBUG
            x.WindowState = WindowState.Normal;
            var ScreenNumber = 2;
            if (Screen.AllScreens.Length >= ScreenNumber)
            {
                var screenBounds = Screen.AllScreens[ScreenNumber - 1].Bounds;
                x.WindowStartupLocation = WindowStartupLocation.Manual;
                x.Left = screenBounds.Left;
                x.Top = screenBounds.Top;
                x.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
#endif
        }

        protected override void OnExit(ExitEventArgs e)
        {
            var serviceLocator = ServiceLocator.Default;
            serviceLocator.ResolveType<IRuntimeConfigurationService>().Save();
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

            mapi.AddRecipient("PresetMagician Support", "support@presetmagician.com", false);

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
            mapi.Send("PresetMagician Crash: " + e.Exception.Message, noteText);

            e.Result = true;
        }
    }
}