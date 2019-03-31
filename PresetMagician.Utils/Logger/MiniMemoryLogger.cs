using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace PresetMagician.Utils.Logger
{
    public class MiniMemoryLogger : MiniLogger
    {
        private readonly bool _logToConsole;

        public MiniMemoryLogger(bool logToConsole = false)
        {
            _logToConsole = logToConsole;
        }

        public readonly List<string> LogList = new List<string>();
        

        public override void Write(LogEntry logEntry)
        {
            var logMessage = GetLogEntryAsText(logEntry);
            LogList.Add(logMessage);
            
            

            if (_logToConsole)
            {
                System.Diagnostics.Debug.WriteLine(logMessage);
            }
            
            base.Write(logEntry);
        }
    }
}