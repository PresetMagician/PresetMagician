using System;
using System.Collections.Generic;

namespace PresetMagician.Utils.Logger
{
    public class MiniConsoleLogger: MiniMemoryLogger
    {
        protected HashSet<LogLevel> LogLevelFilter = null;

        public void SetConsoleLogLevelFilter(HashSet<LogLevel> filter)
        {
            LogLevelFilter = filter;
        }
        public override void Write(LogEntry logEntry)
        {
            if (IsFiltered(logEntry))
            {
                return;
            }
            
            var logMessage = GetLogEntryAsText(logEntry);
            
            Output(logMessage);
            base.Write(logEntry);
        }

        protected virtual void Output(string message)
        {
            Console.WriteLine(message);
        }
        
        protected bool IsFiltered(LogEntry logEntry)
        {
            if (LogLevelFilter != null)
            {
                if (!LogLevelFilter.Contains(logEntry.LogLevel))
                {
                    return true;
                }
            }

            return false;
        }
    }
}