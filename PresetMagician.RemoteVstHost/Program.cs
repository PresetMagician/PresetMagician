using System;
using System.Diagnostics;
using System.Reflection;

namespace PresetMagician.ProcessIsolation
{
    internal static class MethodTimeLogger
    {
        public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
        {
            if (elapsed.TotalMilliseconds > 100)
            {
                Debug.WriteLine($"OVER9000 {methodBase.ReflectedType.Name}.{methodBase.Name} {elapsed.TotalMilliseconds}");
            }
        }
    }
    public class Program
    {
       
        [STAThread]
        public static void Main(string[] args)
        {
            App.Main();
        }
    }
}