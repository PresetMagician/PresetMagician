using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Anotar.Catel;
using Castle.DynamicProxy;
using MethodTimer;
using PresetMagician.Core.Interfaces;
using PresetMagician.Core.Models;
using Timer = System.Timers.Timer;
using Type = System.Type;

namespace PresetMagician.RemoteVstHost.Processes
{
    public class VstHostProcess : HostProcess, IVstHostProcess
    {
        private string _address;
        private IRemoteVstService _vstService;
        private Timer _pingTimer;
        private bool _isLocked;
        private bool _vstServiceAvailable;
        private Plugin _lockedToPlugin;
        private readonly object _operationLock = new object();
        private const int shutdownAfterNumUnloads = 8;
        private int currentUnloadCount;
        public ProcessOperation CurrentOperation { get; private set; }
        private ProcessOperation _currentOperation;


        public VstHostProcess(int maxStartupTimeSeconds = 20, bool debug = false) : base(maxStartupTimeSeconds, debug)
        {
        }


        public void WaitUntilStarted()
        {
            var waitCounter = 0;

            while (!_vstServiceAvailable)
            {
                waitCounter++;
                Thread.Sleep(100);

                if (waitCounter > 20)
                {
                    throw new Exception("Did not start within the timeout period");
                }
            }
        }

        public void Lock(Plugin plugin)
        {
            lock (_operationLock)
            {
                IsBusy = true;
                _isLocked = true;
            }

            _lockedToPlugin = plugin;
            Logger.Debug($"Locking to plugin {plugin.PluginName}, see the plugin log for all further information");
        }

        public bool IsLockedToPlugin()
        {
            return _lockedToPlugin != null;
        }

        public Plugin GetLockedPlugin()
        {
            return _lockedToPlugin;
        }

        public void Unlock()
        {
            lock (_operationLock)
            {
                _isLocked = false;
                IsBusy = false;
            }

            Logger.Debug($"Unlocking from  plugin {_lockedToPlugin.PluginName}, logs are coming back to this process");
            _lockedToPlugin = null;

            currentUnloadCount++;
            if (currentUnloadCount > shutdownAfterNumUnloads)
            {
                ForceStop($"Regular Shutdown after {shutdownAfterNumUnloads} unloaded plugins");
            }
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

        public void ResetPingTimer()
        {
            _pingTimer.Stop();
            _pingTimer.Start();
        }

        public bool StartOperation(string operation)
        {
            lock (_operationLock)
            {
                if (CurrentProcessState == ProcessState.EXITED)
                {
                    LogTo.Error(
                        $"Error PID {Pid}: Attempted to start operation {operation} but the process has already stopped!");
                    return false;
                }

                if (_currentOperation != null && !_currentOperation.Completed)
                {
                    LogTo.Error(
                        $"Error PID {Pid}: Attempted to start operation {operation} before another one finished!");
                }

                _currentOperation = new ProcessOperation(operation, DateTime.Now);

                if (!_isLocked)
                {
                    IsBusy = true;
                }
            }

            return true;
        }

        public void StopOperation(string operation, string result = "OK")
        {
            lock (_operationLock)
            {
                if (_currentOperation == null)
                {
                    LogTo.Error(
                        $"Error PID {Pid}: Attempted to stop operation {operation} but there was no operation running!");
                    return;
                }

                if (_currentOperation.Name != operation)
                {
                    LogTo.Warning(
                        $"Warning PID {Pid}: Attempted to stop operation {operation} but the current running operation is {_currentOperation.Name}");
                }

                if (!_isLocked)
                {
                    IsBusy = false;
                }
            }

            _currentOperation.SetStopTime(DateTime.Now);
            _currentOperation.SetResult(result);
            _currentOperation.SetCompleted();
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
                ForceStop($"Ping failed: {ex}");
            }
        }

        public bool IsRemoteVstServiceAvailable()
        {
            return _vstServiceAvailable;
        }

        protected override void OnBeforeForceStop()
        {
            _pingTimer.Stop();
            _vstServiceAvailable = false;

            if (_vstService != null)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                var castedVstService = _vstService as IChannel;

                if (castedVstService.State == CommunicationState.Faulted)
                {
                    castedVstService.Abort();
                }
            }

            base.OnBeforeForceStop();
        }

        public virtual IRemoteVstService GetVstService()
        {
            return _vstService;
        }

        private ProxyGenerator _generator = new ProxyGenerator();


        protected override void OnAfterStart()
        {
            try
            {
                _address = Constants.BaseAddress + Pid;

                var binding = WcfUtils.GetNetNamedPipeBinding();

                var ep = new EndpointAddress(_address);

                var type = typeof(IRemoteVstService);
                var proxyCreatorType = MakeGenericType(type);
                var proxyCreator = GetProxyCreator(proxyCreatorType, binding, ep);
                var x = _generator.CreateInterfaceProxyWithoutTarget(type, new[] {typeof(IContextChannel)},
                    CreateInterceptor(proxyCreator, type));

                _vstService = x as IRemoteVstService;


                _vstService.Ping();
                _vstServiceAvailable = true;
                IsBusy = false;
                _pingTimer.Start();
            }
            catch (Exception ex)
            {
                ForceStop($"Failure when attempting to open the service channel: {ex}");
            }
        }

        protected override void OnOutputDataReceived(string data)
        {
            if (IsLockedToPlugin() && GetLockedPlugin() != null)
            {
                GetLockedPlugin().Logger.Debug($"Console {Pid}: {data}");
            }
        }

        private IInterceptor CreateInterceptor(IChannelFactory proxyCreator, Type serviceType)
        {
            dynamic channelFactory = proxyCreator;
            return new ClientProxyInterceptor(() => (ICommunicationObject) channelFactory.CreateChannel(), serviceType,
                this);
        }

        private IChannelFactory CreateProxyInstanceCreator(Type genericProxyCreatorType, Binding binding,
            EndpointAddress endpointAddress)
        {
            var proxyInstanceCreator =
                (IChannelFactory) Activator.CreateInstance(genericProxyCreatorType, binding, endpointAddress);
            return proxyInstanceCreator;
        }

        private IChannelFactory GetProxyCreator(Type genericProxyCreatorType, Binding binding,
            EndpointAddress endpointAddress)
        {
            return CreateProxyInstanceCreator(genericProxyCreatorType, binding, endpointAddress);
        }

        private Type MakeGenericType(Type serviceType)
        {
            if (!serviceType.IsInterface)
            {
                throw new InvalidOperationException(
                    "Type returned from proxy type provider must not be concrete type!");
            }


            return typeof(ChannelFactory<>).MakeGenericType(serviceType);
        }
    }
}