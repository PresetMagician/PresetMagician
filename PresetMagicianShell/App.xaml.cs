using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Services;
using Orchestra.Services;
using Orchestra.Views;
using PresetMagicianShell.Services;
using PresetMagicianShell.Services.Interfaces;
using PresetMagicianShell.ViewModels;
using PresetMagicianShell.Views;

namespace PresetMagicianShell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constants

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        #endregion Constants

        protected override async void OnStartup(StartupEventArgs e)
        {
            
            
#if DEBUG
            LogManager.AddDebugListener(true);
#endif

            var fileLogListener = new FileLogListener {IgnoreCatelLogging = true, FilePath = @"{AppDataLocal}\{AutoLogFileName}"};

            LogManager.AddListener(fileLogListener);

            var languageService = ServiceLocator.Default.ResolveType<ILanguageService>();
            languageService.PreferredCulture = new CultureInfo("en-US");
            languageService.FallbackCulture = new CultureInfo("en-US");


            StartShell();
           



        }

       

        private async void StartShell()
        {
            var serviceLocator = ServiceLocator.Default;
            var shellService = serviceLocator.ResolveType<IShellService>();

            var x = await shellService.CreateAsync<ShellWindow>();

            // Overrides
            x.Title = "PresetMagician";
            x.TitleBackground = (Brush)x.FindResource("AccentColorBrush");
            x.TitleForeground = (Brush)x.FindResource("WhiteBrush");
            x.TitleBarHeight = 24;
        }
        
        protected override void OnExit(ExitEventArgs e)
        {

            var serviceLocator = ServiceLocator.Default;
            serviceLocator.ResolveType<IRuntimeConfigurationService>().Save();
            base.OnExit(e);
        }
    }
}