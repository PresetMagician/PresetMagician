using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
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
        //private static FileLogListener _fileLogListener;

        protected override void OnStartup(StartupEventArgs e)
        {
           /* _fileLogListener = new FileLogListener
            {
                IgnoreCatelLogging = false,
                FilePath = @"{AppDataLocal}\Logs\PresetMagician.RemoteVstHost"+Process.GetCurrentProcess().Id+".log",
                TimeDisplay = TimeDisplay.DateTime
            };

            LogManager.AddListener(_fileLogListener);*/
            
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            
           string address = Constants.BaseAddress + Process.GetCurrentProcess().Id;

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

            //LogTo.Debug("Starting up");
            //Console.WriteLine($"{address} ready.");
            Console.WriteLine($"PresetMagician.RemoteVstHost.exe:{Process.GetCurrentProcess().Id} ready.");
            base.OnStartup(e);
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var exception = e.Exception;
            //LogTo.Debug("Got exception "+exception.Message);
            //LogTo.Debug(exception.StackTrace);
            //_fileLogListener.FlushAsync().Wait();
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception) e.ExceptionObject;
            //LogTo.Debug("Got exception "+exception.Message + ", is terminating: "+e.IsTerminating);
            //LogTo.Debug(exception.StackTrace);
            //_fileLogListener.FlushAsync().Wait();
        }

        private void OnIdleTimeout(object sender, ElapsedEventArgs e)
        {
            //LogTo.Debug("Idle for 120 seconds, shutting down");
            //_fileLogListener.FlushAsync().Wait();
            Process.GetCurrentProcess().Kill();
        }

        public static void Ping()
        {
            _shutdownTimer.Stop();
            _shutdownTimer.Start();
        }

        private void ServiceHostOnFaulted(object sender, EventArgs e)
        {
            //LogTo.Debug("Service Host faulted, exiting");

            _serviceHost.Close();
            //_fileLogListener.FlushAsync().Wait();
            Process.GetCurrentProcess().Kill();
        }

        public static void KillSelf()
        {
            //LogTo.Debug("I should kill myself now");
            //_fileLogListener.FlushAsync().Wait();
            //File.Delete(_fileLogListener.FilePath);
            //Process.GetCurrentProcess().Kill();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //LogTo.Debug("Regular shutdown");
            _serviceHost.Close();
            //_fileLogListener.FlushAsync().Wait();
            //File.Delete(_fileLogListener.FilePath);
            base.OnExit(e);
        }
    }
}