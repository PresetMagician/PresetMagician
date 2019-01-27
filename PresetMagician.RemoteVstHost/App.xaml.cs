using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Timers;
using System.Windows;
using PresetMagician.ProcessIsolation.Services;
using SharedModels;

namespace PresetMagician.ProcessIsolation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private ServiceHost _serviceHost;

        private static Timer _shutdownTimer;

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            string address = ProcessPool.BaseAddress + Process.GetCurrentProcess().Id;

            _serviceHost = new ServiceHost(typeof(RemoteVstService));
            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

            var dummyWin = new Window();
            Current.MainWindow = dummyWin;
            _serviceHost.AddServiceEndpoint(typeof(IRemoteVstService), binding, address);
            _serviceHost.Open();
            _serviceHost.Faulted += ServiceHostOnFaulted;

            _shutdownTimer = new Timer();
            _shutdownTimer.Elapsed += OnIdleTimeout;
            _shutdownTimer.Interval = 60 * 2 * 1000; // 2 minutes idle timeout
            _shutdownTimer.Enabled = true;
            _shutdownTimer.AutoReset = false;

            Console.WriteLine("Ready.");
            base.OnStartup(e);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Got exception: " + ((Exception) e.ExceptionObject).ToString());
        }

        private void OnIdleTimeout(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Idle for 120 seconds, shutting down");
            Process.GetCurrentProcess().Kill();
        }

        public static void Ping()
        {
            _shutdownTimer.Stop();
            _shutdownTimer.Start();
        }

        private void ServiceHostOnFaulted(object sender, EventArgs e)
        {
            Console.WriteLine("Service Host faulted, exiting");

            _serviceHost.Close();
            Process.GetCurrentProcess().Kill();
        }


        protected override void OnExit(ExitEventArgs e)
        {
            _serviceHost.Close();
            base.OnExit(e);
        }
    }
}