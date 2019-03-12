using Catel.MVVM;

namespace Drachenkatze.PresetMagician.Utils.Progress
{
    public class StringProgress : ITaskProgressReport
    {
        #region Constructors

        public StringProgress(string status = null)
        {
            Status = status;
        }

        #endregion

        #region Properties

        public string Status { get; private set; }

        #endregion
    }
}