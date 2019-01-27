using System.Windows;

namespace Microsoft.DwayneNeed.Controls
{
    public interface IModalContent<T>
    {
        T Accept();
        void Cancel();
        FrameworkElement Content { get; }
    }
}