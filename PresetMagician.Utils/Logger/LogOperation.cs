using System;

namespace PresetMagician.Utils.Logger
{
    public class LogOperation
    {
        public Guid Guid { get; } = Guid.NewGuid();
        public string Name { get; set; }
    }
}