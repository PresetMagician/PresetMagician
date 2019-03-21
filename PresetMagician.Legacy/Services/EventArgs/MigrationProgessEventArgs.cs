namespace PresetMagician.Legacy.Services.EventArgs
{
    public class MigrationProgessEventArgs : System.EventArgs
    {
        public MigrationProgessEventArgs(string progress)
        {
            Progress = progress;
        }

        /// <summary>
        /// Gets the new value.
        /// </summary>
        /// <value>The new value.</value>
        public string Progress { get; }
    }
}