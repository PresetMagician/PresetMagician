using System;

namespace PresetMagician.Services
{
    class ApplicationAlreadyBusyException : ApplicationException
    {
        private static string PrefixMessage(string message)
        {
            return $"Application is busy with: {message}";
        }

        public ApplicationAlreadyBusyException(string message) : base(PrefixMessage(message))
        {
        }
    }
}