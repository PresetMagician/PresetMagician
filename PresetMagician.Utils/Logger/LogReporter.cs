using System;
using PresetMagician.Utils.Logger.EventArgs;

namespace PresetMagician.Utils.Logger
{
    public class LogReporter: ILogReporter
    {
        private MiniLogger _logger;

        public LogReporter(MiniLogger logger)
        {
            _logger = logger;
        }

        public event EventHandler<LogEntryAddedEventArgs> LogEntryAdded;

        public void Report(LogEntry entry)
        {
            _logger.Log(entry);
            LogEntryAdded?.Invoke(this, new LogEntryAddedEventArgs(entry));
        }
    }
}