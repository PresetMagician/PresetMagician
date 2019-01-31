using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Timers;
using Anotar.Catel;
using Catel;
using MethodTimer;
using PresetMagician.ProcessIsolation.Services;
using SharedModels;

namespace PresetMagician.ProcessIsolation.Processes
{
    public class VstHostProcess : HostProcess
    {
        private string _address;
        private IRemoteVstService _vstService;
        private ProxiedRemoteVstService _proxiedVstService;
        private Timer _pingTimer;
        
        private readonly object _operationLock = new object();
        
        public event EventHandler OperationUpdated;
        public ProcessOperation CurrentOperation { get; private set; }
        
        public List<ProcessOperation> Operations = new List<ProcessOperation>();
        public VstHostProcess(int maxStartupTimeSeconds = 20) : base(maxStartupTimeSeconds)
        {
            
           
        }

        public override void Start()
        {
            base.Start();
            
            _pingTimer = new Timer();
            _pingTimer.Elapsed += PingTimerOnElapsed;
            _pingTimer.Interval = 60000;
            _pingTimer.Enabled = false;
            _pingTimer.AutoReset = false;
            
        }

        protected bool StartOperation(string operation)
        {
            lock (_operationLock)
            {
                if (CurrentProcessState == ProcessState.EXITED)
                {
                    LogTo.Error(
                        $"Error PID {Pid}: Attempted to start operation {operation} but the process has already stopped!");
                    return false;
                }

                if (IsBusy)
                {
                    LogTo.Error(
                        $"Error PID {Pid}: Attempted to start operation {operation} before another one finished!");
                }

                CurrentOperation = new ProcessOperation(operation, DateTime.Now);
                IsBusy = true;
            }

            Operations.Add(CurrentOperation);
            OperationUpdated.SafeInvoke(this);
            return true;
        }

        protected void StopOperation(string operation, string result = "OK")
        {
            lock (_operationLock)
            {
                if (CurrentOperation == null)
                {
                    LogTo.Error(
                        $"Error PID {Pid}: Attempted to stop operation {operation} but there was no operation running!");
                    return;
                }

                if (CurrentOperation.Name != operation)
                {
                    LogTo.Error(
                        $"Error PID {Pid}: Attempted to stop operation {operation} but the current running operation is {CurrentOperation}");
                    CurrentOperation = null;
                    return;
                }

                IsBusy = false;
                CurrentOperation.SetStopTime(DateTime.Now);
                CurrentOperation.SetResult(result);
                CurrentOperation.SetCompleted();
            }

            OperationUpdated.SafeInvoke(this);
        }

        [Time]
        private void PingTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (inShutdown || CurrentProcessState == ProcessState.EXITED)
            {
                return;
            }
            
            try
            {
                _vstService.Ping();
                _pingTimer.Start();
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                Log(ex.StackTrace);
                ForceStop("Ping failed: {ex}");
            }
        }

        private void OnFaulted(object sender, EventArgs e)
        {
            if (CurrentProcessState == ProcessState.EXITED || inShutdown)
            {
                return;
            }

            ForceStop("Received client channel fault");
        }
        
        private void OnClosing(object sender, EventArgs e)
        {
            Log("Received OnClosing event");
        }
        
        private void OnClosed(object sender, EventArgs e)
        {
            Log("Received OnClosed event");
        }
        
        private void OnOpened(object sender, EventArgs e)
        {
             try
           {
               _vstService.Ping();
               IsBusy = false;
               _pingTimer.Start();
           }
           catch (Exception ex)
           {
               Log(ex.Message);
               Log(ex.StackTrace);
               ForceStop("Initial ping failed: {ex}");
           }
        }

        protected override void OnBeforeForceStop()
        {
            _pingTimer.Stop();

            base.OnBeforeForceStop();
        }

        protected override void OnAfterStart()
        {
            try
            {
                _address = Constants.BaseAddress + Pid;
                var binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None)
                {
                    MaxReceivedMessageSize = 40000000,
                    MaxBufferSize = 40000000,
                    SendTimeout = new TimeSpan(0, 0, 0, 60)
                };

                var ep = new EndpointAddress(_address);
                _vstService = ChannelFactory<IRemoteVstService>.CreateChannel(binding, ep);

                ((IClientChannel) _vstService).Faulted += OnFaulted;
                ((IClientChannel) _vstService).Closed += OnClosed;
                ((IClientChannel) _vstService).Closing += OnClosing;
                ((IClientChannel) _vstService).Opened += OnOpened;
                ((IClientChannel) _vstService).Open();
            }
            catch (Exception ex)
            {
                ForceStop($"Failure when attempting to open the service channel: {ex}");
            }

        }
    }
}