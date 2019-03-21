using System;
using PresetMagician.Utils.Logger.EventArgs;

namespace PresetMagician.Utils.Logger
{
    public interface ILogReporter
    {
        event EventHandler<LogEntryAddedEventArgs> LogEntryAdded;

        void Report(LogEntry entry);
    }
}