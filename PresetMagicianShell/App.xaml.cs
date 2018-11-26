using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Catel.IoC;
using Catel.Logging;
using Orchestra.Services;
using Orchestra.Views;

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

            var serviceLocator = ServiceLocator.Default;

            var shellService = serviceLocator.ResolveType<IShellService>();

            await shellService.CreateAsync<ShellWindow>();
        }
    }
}