using System.IO;

namespace PresetMagician.Utils.Logger
{
    public class MiniDiskLogger : MiniLogger
    {
        private const string _timeFormat = "yyyy-MM-dd HH:mm:ss:fff";
        private static object _fileLock = new object();
        public string LogFilePath { get; }

        public MiniDiskLogger(string logFilePath)
        {
            LogFilePath = logFilePath;
        }


        public override void Write(LogEntry logEntry)
        {
            lock (_fileLock)
            {
                var logMessage =
                    $"{logEntry.DateTime.ToString(_timeFormat)} [{GetLogLevelShortCode(logEntry.LogLevel)}] {logEntry.Message}";

                var logStream = new FileStream(LogFilePath, FileMode.Append);
                var logStreamWriter = new StreamWriter(logStream);
                logStreamWriter.WriteLine(logMessage);
                logStreamWriter.Close();
            }
        }
    }
}