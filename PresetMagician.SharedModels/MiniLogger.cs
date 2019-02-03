using System;
using System.Collections.Generic;
using System.Diagnostics;
using Catel.Logging;

namespace SharedModels
{
    public class MiniLogger: ILog
    {
        private readonly bool _logToConsole;
        public MiniLogger(bool logToConsole = false)
        {
            _logToConsole = logToConsole;
        }

        public List<string> LogList = new List<string>();
        
        public void WriteWithData(string message, object extraData, LogEvent logEvent)
        {
            var logMessage = $"{DateTime.Now.ToLongTimeString()} {message}";
            
            if (_logToConsole)
            {
                Debug.WriteLine(logMessage);
            }
            LogList.Add(logMessage);
        }

        public void WriteWithData(string message, LogData logData, LogEvent logEvent)
        {
            var logMessage = $"{DateTime.Now.ToLongTimeString()} {message}";
            
            if (_logToConsole)
            {
                Debug.WriteLine(logMessage);
            }
            LogList.Add(logMessage);
        }

        public void Indent()
        {
            
        }

        public void Unindent()
        {
        }

        public string Name { get; }
        public System.Type TargetType { get; }
        public object Tag { get; set; }
        public bool IsCatelLogging { get; }
        public int IndentSize { get; set; }
        public int IndentLevel { get; set; }
        public event EventHandler<LogMessageEventArgs> LogMessage;
    }
}