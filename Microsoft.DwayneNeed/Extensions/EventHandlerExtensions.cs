using System;

namespace Microsoft.DwayneNeed.Extensions
{
    public static class EventHandlerExtensions
    {
        public static void RaiseEvent<T>(this EventHandler<T> handler, object sender, T args) where T : EventArgs
        {
            if (handler != null)
            {
                handler(sender, args);
            }
        }
    }
}