using System;
using System.Collections.Generic;
using System.Diagnostics;
using Catel.Logging;

namespace Drachenkatze.PresetMagician.Utils
{
    public class MiniMemoryLogger: MiniLogger
    {
        private const string _timeFormat = "yyyy-MM-dd HH:mm:ss:fff";
        private readonly bool _logToConsole;
        public MiniMemoryLogger(bool logToConsole = false)
        {
            _logToConsole = logToConsole;
        }

        public List<string> LogList = new List<string>();

        public override void Write(string message, LogLevel logLevel)
        {
            var logMessage = $"{DateTime.Now.ToString(_timeFormat)} [{GetLogLevelShortCode(logLevel)}] {message}";
            LogList.Add(logMessage);

            if (_logToConsole)
            {
                System.Diagnostics.Debug.WriteLine(logMessage);
            }
        }
    }
}