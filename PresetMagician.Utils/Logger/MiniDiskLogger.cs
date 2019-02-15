using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Catel.Logging;

namespace Drachenkatze.PresetMagician.Utils
{
    public class MiniDiskLogger: MiniLogger
    {
        private const string _timeFormat = "yyyy-MM-dd HH:mm:ss:fff";
        public string LogFilePath { get; }
        public MiniDiskLogger(string logFilePath)
        {
            LogFilePath = logFilePath;
        }

      
        public override void Write(string message, LogLevel logLevel)
        {
            var logMessage = $"{DateTime.Now.ToString(_timeFormat)} [{GetLogLevelShortCode(logLevel)}] {message}";
            
            var logStream = new FileStream(LogFilePath, FileMode.Append);
            var logStreamWriter = new StreamWriter(logStream);
            logStreamWriter.WriteLine(logMessage);
            logStreamWriter.Close();
        }
    }
}