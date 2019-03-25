using System;
using System.Threading.Tasks;
using Catel.Services;
using Catel.Threading;

namespace PresetMagician.Tests
{
    public class StubDispatcherService: IDispatcherService
    {
        public Task InvokeAsync(Action action)
        {
            action.Invoke();
            return Task.CompletedTask;
        }

        public Task InvokeAsync(Delegate method, params object[] args)
        {
            method.DynamicInvoke(args);
            return Task.CompletedTask;
        }

        public Task<T> InvokeAsync<T>(Func<T> func)
        {
            var result = func.Invoke();

            return Task<T>.FromResult(result);

        }

        public void Invoke(Action action, bool onlyInvokeWhenNoAccess = true)
        {
            action.Invoke();
        }

        public void BeginInvoke(Action action, bool onlyBeginInvokeWhenNoAccess = true)
        {
            action.Invoke();
        }
    }
}