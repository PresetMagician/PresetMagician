using System;
using System.Diagnostics;

namespace PresetMagician.ProcessIsolation
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ConsoleTraceListener consoleTracer;
            consoleTracer = new ConsoleTraceListener();
            Trace.Listeners.Add(consoleTracer);
            App.Main();
        }
    }
}