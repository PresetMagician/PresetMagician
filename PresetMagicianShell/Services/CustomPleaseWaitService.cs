using Catel;
using Catel.Logging;
using Catel.Services;
using Orchestra.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PresetMagicianShell.Services
{
    class CustomPleaseWaitService: Orchestra.Services.PleaseWaitService
    {
        #region Constants
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        #endregion

        #region Constructors
        public CustomPleaseWaitService(IDispatcherService dispatcherService):base(dispatcherService)
        {
           
        }
        #endregion

        public int ShowCounter { get; private set; }

        #region IPleaseWaitService Members
        public virtual void Show(string status = "")
        {
            if (ShowCounter <= 0)
            {
                ShowCounter = 1;
            }

            UpdateStatus(status);
        }

        public virtual void Show(PleaseWaitWorkDelegate workDelegate, string status = "")
        {
            Show(status);

            try
            {
                workDelegate();
            }
            finally
            {
                Hide();
            }
        }

        public virtual void UpdateStatus(string status)
        {
            if (!string.IsNullOrWhiteSpace(status))
            {
                Log.Info(status);
            }
        }

        public virtual void UpdateStatus(int currentItem, int totalItems, string statusFormat = "")
        {
            // not required
        }

        public virtual void Hide()
        {
            ShowCounter = 0;

        }

        public virtual void Push(string status = "")
        {
            if (ShowCounter == 0)
            {
                Show(status);
            }
            else
            {
                ShowCounter++;
            }

            Log.Debug($"Pushed busy indicator, counter is '{ShowCounter}'");
        }

        public virtual void Pop()
        {
            ShowCounter--;

            Log.Debug($"Popped busy indicator, counter is '{ShowCounter}'");

            if (ShowCounter <= 0)
            {
                Hide();
            }

        }
        #endregion
    
}
}
