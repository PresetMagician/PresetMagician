// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationChangedEventArgs.cs" company="Catel development team">
//   Copyright (c) 2008 - 2015 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PresetMagician.Services.EventArgs
{
    /// <summary>
    /// The configuration changed event args class.
    /// </summary>
    public class StartupActionChangedEventArgs : System.EventArgs
    {
        public StartupActionChangedEventArgs(string newValue)
        {
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the new value.
        /// </summary>
        /// <value>The new value.</value>
        public string NewValue { get; private set; }
    }
}