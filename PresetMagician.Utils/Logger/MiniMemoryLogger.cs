using System.Collections.Generic;

namespace PresetMagician.Utils.Logger
{
    public class MiniMemoryLogger : MiniLogger
    {
        private const string _timeFormat = "yyyy-MM-dd HH:mm:ss:fff";
        private readonly bool _logToConsole;

        public MiniMemoryLogger(bool logToConsole = false)
        {
            _logToConsole = logToConsole;
        }

        public List<string> LogList = new List<string>();

        public override void Write(LogEntry logEntry)
        {
            var logMessage =
                $"{logEntry.DateTime.ToString(_timeFormat)} [{GetLogLevelShortCode(logEntry.LogLevel)}] {logEntry.Message}";
            LogList.Add(logMessage);

            if (_logToConsole)
            {
                System.Diagnostics.Debug.WriteLine(logMessage);
            }
        }
    }
}