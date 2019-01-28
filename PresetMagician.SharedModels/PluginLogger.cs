using System;
using System.Collections.Generic;
using Catel.Logging;

namespace SharedModels
{
    public class PluginLogger: ILog
    {
        public List<string> LogList = new List<string>();
        
        public void WriteWithData(string message, object extraData, LogEvent logEvent)
        {
            LogList.Add(message);
        }

        public void WriteWithData(string message, LogData logData, LogEvent logEvent)
        {
            LogList.Add(message);
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