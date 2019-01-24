using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using PresetMagician.ProcessIsolation.Services;
using PresetMagician.VstHost;
using SharedModels;

namespace PresetMagician.ProcessIsolation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceHost _serviceHost;
    

        protected override void OnStartup(StartupEventArgs e)
        {
            string address = ProcessPool.BaseAddress + Process.GetCurrentProcess().Id;

            _serviceHost = new ServiceHost(typeof(RemoteVstService));
            var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

            var dummyWin = new Window();
            Current.MainWindow = dummyWin;
            _serviceHost.AddServiceEndpoint(typeof(IRemoteVstService), binding, address);
            _serviceHost.Open();
            _serviceHost.Faulted += ServiceHostOnFaulted;
            Console.WriteLine("Ready.");
            base.OnStartup(e);
        }

        private void ServiceHostOnFaulted(object sender, EventArgs e)
        {
            Console.WriteLine("Service Host faulted, exiting");
            
            _serviceHost.Close();
            
            
            Current.Shutdown();
        }


        protected override void OnExit(ExitEventArgs e)
        {
            _serviceHost.Close();
            base.OnExit(e);
        }
    }
}
