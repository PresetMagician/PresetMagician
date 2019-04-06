using System;
using System.Collections.Generic;
using System.Linq;

namespace PresetMagician.Utils.Logger
{
    public abstract class MiniLogger
    {
        private const string TIME_FORMAT = "yyyy-MM-dd HH:mm:ss:fff";
        
        private readonly HashSet<MiniLogger> _mirrors = new HashSet<MiniLogger>();

        private readonly HashSet<LogLevel> _loggedLogLevels = new HashSet<LogLevel>();
        
        protected readonly List<LogEntry> LogEntries = new List<LogEntry>();
        
        public void Log(LogEntry logEntry)
        {
            Write(logEntry);
        }

        public bool HasLoggedEntries(LogLevel logLevel)
        {
            return _loggedLogLevels.Contains(logLevel);
        }

        public List<LogEntry> GetFilteredLogEntries(List<LogLevel> restrictToLogLevels = null)
        {
            if (restrictToLogLevels != null)
            {
                return (from logEntry in LogEntries
                    where restrictToLogLevels.Contains(logEntry.LogLevel)
                    orderby logEntry.DateTime
                    select logEntry).ToList();
            }

            
            return (from logEntry in LogEntries
                orderby logEntry.DateTime
                select logEntry).ToList();
            
        }

        public void Clear()
        {
            LogEntries.Clear();
        }
        public virtual void Write(LogEntry logEntry)
        {
            foreach (var logger in _mirrors)
            {
                logger.Write(logEntry);
            }

            LogEntries.Add(logEntry);
            _loggedLogLevels.Add(logEntry.LogLevel);
        }

        public void MirrorTo(MiniLogger logger)
        {
            _mirrors.Add(logger);
        }

        public void Trace(string message, LogOperation operation = null)
        {
            Write(new LogEntry(LogLevel.Trace, message, operation));
        }

        public void Debug(string message, LogOperation operation = null)
        {
            Write(new LogEntry(LogLevel.Debug, message, operation));
        }

        public void Info(string message, LogOperation operation = null)
        {
            Write(new LogEntry(LogLevel.Info, message, operation));
        }

        public void Warning(string message, LogOperation operation = null)
        {
            Write(new LogEntry(LogLevel.Warning, message, operation));
        }

        public void Error(string message, LogOperation operation = null)
        {
            Write(new LogEntry(LogLevel.Error, message, operation));
        }

        public void LogException(Exception ex, LogOperation operation = null, LogLevel logLevel = LogLevel.Debug)
        {
            Write(new LogEntry(logLevel, $"{ex.GetType().FullName}: {ex.Message}{Environment.NewLine}{ex.StackTrace}", operation));
        }

        public string GetLogLevelShortCode(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Info:
                    return "I";
                case LogLevel.Debug:
                    return "D";
                case LogLevel.Error:
                    return "E";
                case LogLevel.Warning:
                    return "W";
                case LogLevel.Trace:
                    return "T";
            }

            return "";
        }

        public string GetLogEntryAsText(LogEntry logEntry)
        {
            var operation = logEntry.Operation != null ? logEntry.Operation.Guid.ToString() : "<none>";
            return
                $"{logEntry.DateTime.ToString(TIME_FORMAT)} [{GetLogLevelShortCode(logEntry.LogLevel)}] [{operation}] {logEntry.Message}";
        }
        
        
    }
}