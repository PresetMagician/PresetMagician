using System;

namespace PresetMagician.Core.Exceptions
{
    public class ConnectivityLostException: Exception
    {
        public ConnectivityLostException(string message) : base(message)
        {
        }

        public ConnectivityLostException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}