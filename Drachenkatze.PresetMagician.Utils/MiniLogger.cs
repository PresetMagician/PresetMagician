namespace Drachenkatze.PresetMagician.Utils
{
    public enum LogLevel
    {
        /// <summary>Debug message.</summary>
        Debug,
        /// <summary>Info message.</summary>
        Info,
        /// <summary>Warning message.</summary>
        Warning,
        /// <summary>Error message.</summary>
        Error
    }
    
    public abstract class MiniLogger
    {
        
        public abstract void Write(string message, LogLevel logLevel);

        public void Debug(string message)
        {
            Write(message, LogLevel.Debug);
        }
        
        public void Info(string message)
        {
            Write(message, LogLevel.Info);
        }
        
        public void Warning(string message)
        {
            Write(message, LogLevel.Warning);
        }
        
        public void Error(string message)
        {
            Write(message, LogLevel.Error);
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
            }

            return "";
        }
        
    }
}