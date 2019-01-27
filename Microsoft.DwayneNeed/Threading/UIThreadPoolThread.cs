using System;
using System.Windows.Threading;

namespace Microsoft.DwayneNeed.Threading
{
    public sealed class UIThreadPoolThread : IDisposable
    {
        internal UIThreadPoolThread(Dispatcher dispatcher)
        {
            UIThreadPool.IncrementThread(dispatcher);
            Dispatcher = dispatcher;
        }

        ~UIThreadPoolThread()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (Dispatcher == null)
            {
                throw new ObjectDisposedException("UIThreadPoolThread");
            }

            Dispatcher dispatcher = Dispatcher;
            Dispatcher = null;

            UIThreadPool.DecrementThread(dispatcher);
        }

        public Dispatcher Dispatcher { get; private set; }
    }
}