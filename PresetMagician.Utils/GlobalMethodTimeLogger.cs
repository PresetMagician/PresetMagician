using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PresetMagician.Utils
{
    public static class MethodTimeLogger
    {
         
        public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
        {
            GlobalMethodTimeLogger.Log(methodBase, elapsed);
           
        }

        
    }
    
    public static class GlobalMethodTimeLogger
    {
        private static readonly object _lock = new object();
        public static readonly Dictionary<string, GlobalMethodTimeLoggerEntry> MethodTimings = new Dictionary<string, GlobalMethodTimeLoggerEntry>();
        public static void Log(MethodBase methodBase, TimeSpan elapsed)
        {
            
            var methodName = $"{methodBase.ReflectedType.Name}.{methodBase.Name}";

            lock (_lock)
            {
                if (!MethodTimings.ContainsKey(methodName))
                {
                    MethodTimings.Add(methodName,
                        new GlobalMethodTimeLoggerEntry() {Method = methodBase, Name = methodName});
                }
            }

            MethodTimings[methodName].CallCount++;
            MethodTimings[methodName].Duration = MethodTimings[methodName].Duration.Add(elapsed); 
           
        }
        
        public static List<GlobalMethodTimeLoggerEntry> GetTopMethods()
        {
            return (from methodTiming in MethodTimings
                    orderby methodTiming.Value.Duration descending
                    select methodTiming.Value
                ).Take(10).ToList();

        }
    }

    public class GlobalMethodTimeLoggerEntry
    {
        public string Name { get; set; }
        public long CallCount { get; set; }
        public MethodBase Method { get; set; }
        public TimeSpan Duration { get; set; }
    }
}