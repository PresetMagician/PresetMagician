using System;
using PresetMagician.Utils.Logger.EventArgs;
using Redmine.Net.Api.Types;

namespace PresetMagician.Utils.Logger
{
    public interface ILogReporter
    {
        event EventHandler<LogEntryAddedEventArgs> LogEntryAdded;
        
        void Report(LogEntry entry);
    }
}