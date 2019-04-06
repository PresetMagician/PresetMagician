using System.IO;

namespace PresetMagician.Utils.Logger
{
    public class MiniDiskLogger : MiniMemoryLogger
    {
        private static readonly object _fileLock = new object();
        public string LogFilePath { get; }

        public MiniDiskLogger(string logFilePath)
        {
            LogFilePath = logFilePath;
        }


        public override void Write(LogEntry logEntry)
        {
            lock (_fileLock)
            {
                var logMessage = GetLogEntryAsText(logEntry);

                var logStream = new FileStream(LogFilePath, FileMode.Append);
                var logStreamWriter = new StreamWriter(logStream);
                logStreamWriter.WriteLine(logMessage);
                logStreamWriter.Close();
            }
            
            base.Write(logEntry);
        }
        
        
    }
}