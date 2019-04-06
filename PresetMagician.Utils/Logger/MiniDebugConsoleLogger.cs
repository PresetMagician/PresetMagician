using System;

namespace PresetMagician.Utils.Logger
{
    public class MiniDebugConsoleLogger: MiniConsoleLogger
    {
        protected override void Output(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}