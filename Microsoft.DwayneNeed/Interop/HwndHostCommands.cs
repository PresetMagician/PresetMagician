using Microsoft.DwayneNeed.Input;

namespace Microsoft.DwayneNeed.Interop
{
    public static class HwndHostCommands
    {
        public static RoutedCommand<MouseActivateParameter> MouseActivate =
            new RoutedCommand<MouseActivateParameter>("MouseActivate", typeof(HwndHostCommands));
    }
}