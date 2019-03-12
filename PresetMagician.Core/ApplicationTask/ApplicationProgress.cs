using System;
using System.Threading;
using PresetMagician.Utils.Logger;
using PresetMagician.Utils.Progress;

namespace PresetMagician.Core.ApplicationTask
{
    public class ApplicationProgress
    {
        public ILogReporter LogReporter;
        public IProgress<CountProgress> Progress;
        public CancellationToken CancellationToken;
    }
}