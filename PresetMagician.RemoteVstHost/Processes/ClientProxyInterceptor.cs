using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using Anotar.Catel;
using Castle.DynamicProxy;
using PresetMagician.Core.Exceptions;

namespace PresetMagician.RemoteVstHost.Processes
{
    public class ClientProxyInterceptor : IInterceptor
    {
        private readonly Func<ICommunicationObject> proxyCreator;
        private readonly Type typeToProxy;
        private ICommunicationObject proxyInstance;
        private readonly VstHostProcess _vstHostProcess;

        public ClientProxyInterceptor(Func<ICommunicationObject> proxyCreator, Type typeToProxy,
            VstHostProcess vstHostProcess)
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
            if (invocation.Method.Name != "get_State")
            {
                _vstHostProcess.StartOperation(invocation.Method.Name);
                _vstHostProcess.ResetPingTimer();
            }


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
                var innerException = ex.InnerException;

                if (innerException == null)
                {
                    LogTo.Warning(
                        $"Got a TargetInvocationException but the innerException was null. Please report this as a bug :) Message: {ex.Message}");
                    LogTo.Debug(ex.StackTrace);
                }

                if (innerException is CommunicationException && !(innerException is FaultException))
                {
                    var message =
                        $"Connectivity to remote VST host lost. Most likely reason: Plugin crashed. {innerException.GetType().FullName}: {innerException.Message}";
                    // Didn't get a FaultException, most likely a channel fault. Stop the VST host process
                    _vstHostProcess.ForceStop(message);

                    throw new ConnectivityLostException(message, innerException);
                }

                var exceptionType = innerException.GetType();

                if (exceptionType.IsGenericType && innerException is FaultException)
                {
                    // Find the type of the generic parameter
                    var genericType = exceptionType.GetGenericArguments().FirstOrDefault();

                    if (genericType == null)
                    {
                        LogTo.Warning(
                            $"Got generic exception: {innerException.GetType().FullName}: {innerException.Message}. Please report this!");
                        LogTo.Debug(innerException.StackTrace);

                        if (_vstHostProcess.IsLockedToPlugin())
                        {
                            _vstHostProcess.GetLockedPlugin().Logger
                                .Warning(
                                    $"Got generic exception: {innerException.GetType().FullName}: {innerException.Message}. Please report this!");
                            _vstHostProcess.GetLockedPlugin().Logger
                                .Debug(innerException.StackTrace);
                        }
                    }
                }

                throw innerException;
            }
            finally
            {
                if (_vstHostProcess.IsLockedToPlugin())
                {
                    _vstHostProcess.GetLockedPlugin().Logger
                        .Trace(
                            $"{invocation.Method.Name}({string.Join(",", argumentList)}): {ValueToString(invocation.ReturnValue)}");
                }
                else
                {
                    _vstHostProcess.Logger.Trace(
                        $"{invocation.Method.Name}({string.Join(",", argumentList)}): {ValueToString(invocation.ReturnValue)}");
                }

                if (invocation.Method.Name != "get_State")
                {
                    _vstHostProcess.StopOperation(invocation.Method.Name);
                }
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