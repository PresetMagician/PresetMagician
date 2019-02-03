using System;
using System.IO;
using System.Runtime;
using Catel.IoC;
using Orchestra.Services;
using PresetMagician.Services;
using SplashScreenService = PresetMagician.Services.SplashScreenService;

namespace PresetMagician
{
    public class Program
    {
        public static DateTime startTime;

        [STAThread]
        public static void Main(string[] args)
        {
            startTime = DateTime.Now;
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var profileRoot = Path.Combine(appData, "Drachenkatze/PresetMagician/Cache");
            Directory.CreateDirectory(profileRoot);
            ProfileOptimization.SetProfileRoot(profileRoot);
            ProfileOptimization.StartProfile("Startup.Profile");

            var serviceLocator = ServiceLocator.Default;

            serviceLocator.RegisterType<ISplashScreenService, SplashScreenService>();
            serviceLocator.RegisterType<IRibbonService, RibbonService>();
            serviceLocator.RegisterType<IApplicationInitializationService, ApplicationInitializationService>();
            App.Main();
        }
    }
}