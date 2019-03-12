namespace PresetMagician.Utils.Logger.EventArgs
{
    public class LogEntryAddedEventArgs : System.EventArgs
    {
        public LogEntryAddedEventArgs(LogEntry logEntry)
        {
            LogEntry = logEntry;
        }

        /// <summary>
        /// Gets the new value.
        /// </summary>
        /// <value>The new value.</value>
        public LogEntry LogEntry { get;  }
    }
}