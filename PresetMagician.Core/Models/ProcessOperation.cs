using System;
using Catel.Data;

namespace PresetMagician.RemoteVstHost.Processes
{
    public class ProcessOperation : ObservableObject
    {
        public ProcessOperation(string name, DateTime startTime)
        {
            Name = name;
            StartTime = startTime;
        }

        public void SetResult(string result)
        {
            Result = result;
        }

        public void SetStopTime(DateTime stopTime)
        {
            StopTime = stopTime;
            _hasStopTime = true;
        }

        public void SetCompleted()
        {
            Completed = true;
        }

        public string Name { get; }
        public string Result { get; private set; }
        public bool Completed { get; private set; }

        public TimeSpan ElapsedTime
        {
            get
            {
                if (!_hasStopTime)
                {
                    return DateTime.Now - StartTime;
                }

                return StopTime - StartTime;
            }
        }

        public DateTime StartTime { get; }
        public DateTime StopTime { get; private set; }
        private bool _hasStopTime;
    }
}