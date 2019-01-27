// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationChangedEventArgs.cs" company="Catel development team">
//   Copyright (c) 2008 - 2015 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using SharedModels;

namespace PresetMagician.Models.EventArgs
{
    /// <summary>
    /// The configuration changed event args class.
    /// </summary>
    public class PresetUpdatedEventArgs : System.EventArgs
    {
        public PresetUpdatedEventArgs(Preset preset)
        {
            NewValue = preset;
        }

        /// <summary>
        /// Gets the new value.
        /// </summary>
        /// <value>The new value.</value>
        public Preset NewValue { get; private set; }
    }
}