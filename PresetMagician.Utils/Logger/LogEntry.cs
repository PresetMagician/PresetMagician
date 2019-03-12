using System;

namespace PresetMagician.Utils.Logger
{
    public enum LogLevel
    {
        /// <summary>Debug message.</summary>
        Debug,
        /// <summary>Info message.</summary>
        Info,
        /// <summary>Warning message.</summary>
        Warning,
        /// <summary>Error message.</summary>
        Error,
        
        Trace
    }
    
    public class LogEntry
    {
        public LogEntry(string message)
        {
            DateTime = DateTime.Now;
            Message = message;
            LogLevel = LogLevel.Info;
        }
        
        public LogEntry(LogLevel logLevel, string message)
        {
            DateTime = DateTime.Now;
            Message = message;
            LogLevel = LogLevel.Info;
        }
        
        

        public LogLevel LogLevel { get; set; }
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
    }
}