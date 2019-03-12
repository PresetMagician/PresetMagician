using PresetMagician.Core.Models;

namespace PresetMagician.Core.EventArgs
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