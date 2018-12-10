namespace Drachenkatze.PresetMagician.VSTHost.VST.EventArgs
{
    public class VstPluginLoadErrorEventArgs : System.EventArgs
    {
        public VstPluginLoadErrorEventArgs(string newValue)
        {
            LoadError = newValue;
        }

        /// <summary>
        ///     Gets the new value.
        /// </summary>
        /// <value>The new value.</value>
        public string LoadError { get; }
    }
}