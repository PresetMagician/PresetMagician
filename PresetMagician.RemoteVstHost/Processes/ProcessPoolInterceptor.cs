using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace PresetMagician.RemoteVstHost.Processes
{
   public class ProcessPoolInterceptor : IInterceptor
    {
        private readonly RemoteVstHostProcessPool _processPool;

        public ProcessPoolInterceptor(RemoteVstHostProcessPool processPool)
        {
            _processPool = processPool;
        }

    

        public void Intercept(IInvocation invocation)
        {
            var process = _processPool.GetFreeHostProcess();
            try
            {
                invocation.ReturnValue = invocation.Method.Invoke(process.GetVstService(), invocation.Arguments);
            }
            catch (TargetInvocationException ex)
            {
                Exception innerException = ex.InnerException;

                throw innerException;
            }
        }
    }
   
  
}