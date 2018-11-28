using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Catel.IoC;
using Catel.Logging;
using Orchestra.Services;
using Orchestra.Views;
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
            LogManager.AddDebugListener(false);
#endif

            var serviceLocator = ServiceLocator.Default;

            var shellService = serviceLocator.ResolveType<IShellService>();

            var x = await shellService.CreateAsync<ShellWindow>();

            x.TitleBackground = (Brush)x.FindResource("AccentColorBrush");
            x.TitleForeground = (Brush)x.FindResource("WhiteBrush");
            x.TitleBarHeight = 24;
        }
    }
}