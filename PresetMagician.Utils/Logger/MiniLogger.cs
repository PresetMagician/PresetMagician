using System;

namespace PresetMagician.Utils.Logger
{
    public abstract class MiniLogger
    {
        public void Log(LogEntry logEntry)
        {
            Write(logEntry);
        }

        public abstract void Write(LogEntry logEntry);

        public void Trace(string message)
        {
            Write(new LogEntry(LogLevel.Trace, message));
        }

        public void Debug(string message)
        {
            Write(new LogEntry(LogLevel.Debug, message));
        }

        public void Info(string message)
        {
            Write(new LogEntry(LogLevel.Info, message));
        }

        public void Warning(string message)
        {
            Write(new LogEntry(LogLevel.Warning, message));
        }

        public void Error(string message)
        {
            Write(new LogEntry(LogLevel.Error, message));
        }

        public void LogException(Exception ex, LogLevel logLevel = LogLevel.Debug)
        {
            Write(new LogEntry(logLevel, $"{ex.GetType().FullName}: {ex.Message}{Environment.NewLine}{ex.StackTrace}"));
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
    }
}