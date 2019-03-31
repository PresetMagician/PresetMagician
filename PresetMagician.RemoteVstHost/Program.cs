using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security;
using PresetMagician.Utils;

namespace PresetMagician.RemoteVstHost
{
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