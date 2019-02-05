using System;
using System.Reflection;
using System.ServiceModel;
using Anotar.Catel;
using Castle.DynamicProxy;

namespace PresetMagician.RemoteVstHost.Processes
{
   public class ClientProxyInterceptor : IInterceptor
    {
        private readonly Func<ICommunicationObject> proxyCreator;
        private readonly Type typeToProxy;
        private ICommunicationObject proxyInstance;
        private readonly VstHostProcess _vstHostProcess;

        public ClientProxyInterceptor(Func<ICommunicationObject> proxyCreator, Type typeToProxy, VstHostProcess vstHostProcess)
        {
            this.typeToProxy = typeToProxy;
            this.proxyCreator = proxyCreator;
            _vstHostProcess = vstHostProcess;
        }

        public ICommunicationObject CachedProxy
        {
            get
            {
                EnsureProxyExists();
                return proxyInstance;
            }
            set => proxyInstance = value;
        }

        public void Intercept(IInvocation invocation)
        {
            _vstHostProcess.StartOperation(invocation.Method.Name);
            _vstHostProcess.ResetPingTimer();

            if (_vstHostProcess.IsLockedToPlugin())
            {
                _vstHostProcess.GetLockedPlugin().Logger.Debug("Calling "+invocation.Method.Name);   
            }

            try
            {
                invocation.ReturnValue = invocation.Method.Invoke(CachedProxy, invocation.Arguments);
                _vstHostProcess.ResetPingTimer();
            }
            catch (TargetInvocationException ex)
            {
                Exception innerException = ex.InnerException;

                if (innerException is CommunicationException && !(innerException is FaultException))
                {
                    _vstHostProcess.ForceStop($"{innerException.GetType().FullName}: {innerException.Message}");
                }
                else
                {
                    LogTo.Debug($"Got exception: {innerException.GetType().FullName}");    
                }
                
                
                throw innerException;
            }
            finally
            {
                _vstHostProcess.StopOperation(invocation.Method.Name);
            }

            
        }

        private void EnsureProxyExists()
        {
            if (proxyInstance == null)
            {
                proxyInstance = proxyCreator();
            }
        }

        

    }
   
  
}