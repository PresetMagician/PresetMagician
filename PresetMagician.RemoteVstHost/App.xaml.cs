using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Security;
using System.ServiceModel;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
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
        private static StreamWriter _logFileStreamWriter;
        private static FileStream _logFileStream;
        private static string _logFile;

        protected override void OnStartup(StartupEventArgs e)
        {
            var path = VstUtils.GetVstWorkerLogDirectory();
            Directory.CreateDirectory(path);

            _logFile = Path.Combine(path, "PresetMagician.RemoteVstHost" +
                                             Process.GetCurrentProcess().Id + ".log");

            
          
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
            
           string address = Constants.BaseAddress + Process.GetCurrentProcess().Id;
           
            _serviceHost = new ServiceHost(typeof(RemoteVstService));
            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

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

        private void ServiceHostOnOpened(object sender, EventArgs e)
        {
            Console.WriteLine($"PresetMagician.RemoteVstHost.exe:{Process.GetCurrentProcess().Id} ready.");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss:fff}] => {Process.GetCurrentProcess().Id} rdy");
        }

        private static void MiniLog(string message)
        {
            _logFileStream = new FileStream(_logFile, FileMode.Append);
            _logFileStreamWriter = new StreamWriter(_logFileStream);
            _logFileStreamWriter.WriteLine($"[{DateTime.Now:HH:mm:ss:fff}] => {message}");
            _logFileStreamWriter.Flush();
            _logFileStreamWriter.BaseStream.Flush();
            _logFileStreamWriter.Close();
            _logFileStream.Close();
        }

        private static void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var exception = e.Exception;
            MiniLog("Got exception "+exception.Message);
            MiniLog(exception.StackTrace);
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = (Exception) e.ExceptionObject;
            MiniLog("Got exception "+exception.Message + ", is terminating: "+e.IsTerminating);
            MiniLog(exception.StackTrace);
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
            MiniLog("Service Host faulted, exiting");
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