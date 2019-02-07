using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.Text;
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

        private string ValueToString(object value)
        {
            if (value == null)
            {
                return "null";
            }
            if (value.GetType().IsPrimitive)
            {
                return value.ToString();
            }

            switch (value.GetType().FullName)
            {
                case "System.Guid":
                    return value.ToString();
             
            }
            
            return value.GetType().FullName;
        }

        public void Intercept(IInvocation invocation)
        {
            _vstHostProcess.StartOperation(invocation.Method.Name);
            _vstHostProcess.ResetPingTimer();

            
            
                var argumentList = new List<string>();
                
                foreach (var argument in invocation.Arguments)
                {
                    argumentList.Add(ValueToString(argument));
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
            
            if (_vstHostProcess.IsLockedToPlugin())
            {
                _vstHostProcess.GetLockedPlugin().Logger.Debug($"{invocation.Method.Name}({string.Join(",",argumentList)}): {ValueToString(invocation.ReturnValue)}");   
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