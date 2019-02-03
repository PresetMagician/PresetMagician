using System;
using System.Reflection;
using System.ServiceModel;
using Anotar.Catel;
using Castle.DynamicProxy;
using Catel.Logging;

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
                this.EnsureProxyExists();
                return this.proxyInstance;
            }
            set
            {
                this.proxyInstance = value;
            }
        }

        public void Intercept(IInvocation invocation)
        {
            _vstHostProcess.StartOperation(invocation.Method.Name);
            _vstHostProcess.ResetPingTimer();

            if (_vstHostProcess.IsLockedToPlugin())
            {
                _vstHostProcess.Logger.Debug(invocation.Method.Name);   
            }

            try
            {
                invocation.ReturnValue = invocation.Method.Invoke(CachedProxy, invocation.Arguments);
                _vstHostProcess.ResetPingTimer();
            }
            catch (TargetInvocationException ex)
            {
                Exception innerException = ex.InnerException;

                if (innerException is CommunicationException)
                {
                    _vstHostProcess.ForceStop($"Communication Exception {innerException.Message}");
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
            if (this.proxyInstance == null)
            {
                this.proxyInstance = this.proxyCreator();
            }
        }

        private void CloseProxy(MethodInfo methodInfo)
        {
            var wcfProxy = this.CachedProxy;

            if (wcfProxy != null && this.typeToProxy.IsAssignableFrom(methodInfo.DeclaringType))
            {
                if (wcfProxy.State == CommunicationState.Faulted)
                {
                    this.AbortCommunicationObject(wcfProxy);
                }
                else if (wcfProxy.State != CommunicationState.Closed)
                {
                    try
                    {
                        wcfProxy.Close();

                        this.CachedProxy = null;
                    }
                    catch (CommunicationException)
                    {
                        this.AbortCommunicationObject(wcfProxy);
                    }
                    catch (TimeoutException)
                    {
                        this.AbortCommunicationObject(wcfProxy);
                    }
                    catch (Exception)
                    {
                        this.AbortCommunicationObject(wcfProxy);
                        throw;
                    }
                }
            }
        }

        private void AbortCommunicationObject(ICommunicationObject wcfProxy)
        {
            wcfProxy.Abort();

            this.CachedProxy = null;
        }
    }
   
  
}