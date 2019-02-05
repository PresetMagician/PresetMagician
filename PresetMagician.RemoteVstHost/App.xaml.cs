using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Catel.Logging;
using Drachenkatze.PresetMagician.Utils;
using PresetMagician.RemoteVstHost.Services;
using SharedModels;

namespace PresetMagician.RemoteVstHost
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static ServiceHost _serviceHost;

        private static Timer _shutdownTimer;
        public static MiniDiskLogger MiniDiskLogger;
        private readonly HashSet<int> _processedExceptions = new HashSet<int>();

        protected override void OnStartup(StartupEventArgs e)
        {
            var path = VstUtils.GetVstWorkerLogDirectory();

            var logFile = Path.Combine(path, "PresetMagician.RemoteVstHost" +
                                             Process.GetCurrentProcess().Id + ".log");

            MiniDiskLogger = new MiniDiskLogger(logFile);
          
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;
            
            string address = Constants.BaseAddress + Process.GetCurrentProcess().Id;
           
            _serviceHost = new ServiceHost(typeof(RemoteVstService));
            var binding = WcfUtils.GetNetNamedPipeBinding();

            var dummyWin = new Window();
            Current.MainWindow = dummyWin;
            
            _serviceHost.AddServiceEndpoint(typeof(IRemoteVstService), binding, address);
            _serviceHost.Faulted += ServiceHostOnFaulted;
            _serviceHost.Opened += ServiceHostOnOpened;
            _serviceHost.Open();

            _shutdownTimer = new Timer();
            _shutdownTimer.Elapsed += OnIdleTimeout;
            _shutdownTimer.Interval = 60 * 2 * 1000; // 2 minutes idle timeout
            _shutdownTimer.Enabled = true;
            _shutdownTimer.AutoReset = false;

            base.OnStartup(e);
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var message = $"TaskSchedulerOnUnobservedTaskException {e.Exception.GetType().FullName}: {e.Exception.Message}";
            Console.WriteLine(message);
            Console.WriteLine(e.Exception.StackTrace);
            
            MiniDiskLogger.Error(message);
            MiniDiskLogger.Debug(e.Exception.StackTrace);
        }

        private void CurrentDomainOnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (_processedExceptions.Contains(e.Exception.GetHashCode()))
            {
                return;
            }
            _processedExceptions.Add(e.Exception.GetHashCode());
            var message = $"FirstChanceException {e.Exception.GetType().FullName}: {e.Exception.Message}";
            Console.WriteLine(message);
            Console.WriteLine(e.Exception.StackTrace);
            
            MiniDiskLogger.Error(message);
            MiniDiskLogger.Debug(e.Exception.StackTrace);
            
        }

        private void ServiceHostOnOpened(object sender, EventArgs e)
        {
            Console.WriteLine($"PresetMagician.RemoteVstHost.exe:{Process.GetCurrentProcess().Id} ready.");
        }

        private static void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var exception = e.Exception;
            var message =
                $"CurrentOnDispatcherUnhandledException: {exception.GetType().FullName}: {exception.Message}";
            Console.WriteLine(message);
            Console.WriteLine(exception.StackTrace);
            MiniDiskLogger.Error(message);
            MiniDiskLogger.Debug(exception.StackTrace);
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception) e.ExceptionObject;
            var message =
                $"CurrentOnDispatcherUnhandledException: {exception.GetType().FullName}: {exception.Message}. IsTerminating: {e.IsTerminating}";
            Console.WriteLine(message);
            Console.WriteLine(exception.StackTrace);
            MiniDiskLogger.Error(message);
            MiniDiskLogger.Debug(exception.StackTrace);
        }

        private void OnIdleTimeout(object sender, ElapsedEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        public static void Ping()
        {
            _shutdownTimer.Stop();
            _shutdownTimer.Start();
        }

        private static void ServiceHostOnFaulted(object sender, EventArgs e)
        {
            MiniDiskLogger.Error("Service Host faulted, exiting");
            Process.GetCurrentProcess().Kill();
        }

        public static void KillSelf()
        {
            _serviceHost.Faulted -= ServiceHostOnFaulted;
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;
            Current.DispatcherUnhandledException -= CurrentOnDispatcherUnhandledException;
            
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss:fff}] => {Process.GetCurrentProcess().Id} Shutting down");
            
            Process.GetCurrentProcess().Kill();
        }

        protected override void OnExit(ExitEventArgs e)
        {
          
            _serviceHost.Close();

            base.OnExit(e);
        }
    }
}