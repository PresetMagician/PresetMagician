using Catel.MVVM;

namespace PresetMagician.Utils.Progress
{
    public class CountProgress : ITaskProgressReport
    {
        #region Constructors

        public CountProgress(int total)
        {
            Total = total;
        }

        #endregion

        #region Properties

        public string Status { get; set; }
        public int Current { get; set; }
        public int Total { get; set; }

        #endregion
    }
}