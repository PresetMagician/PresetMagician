using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;

namespace PresetMagician.RemoteVstHost
{
    internal static class MethodTimeLogger
    {
        public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
        {
            if (elapsed.TotalMilliseconds > 100)
            {
                Debug.WriteLine(
                    $"OVER9000 {methodBase.ReflectedType.Name}.{methodBase.Name} {elapsed.TotalMilliseconds}");
            }
        }
    }

    public class Program
    {
        [STAThread]
        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        public static void Main(string[] args)
        {
            App.Main();
        }
    }
}