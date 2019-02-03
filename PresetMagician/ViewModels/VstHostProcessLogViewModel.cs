using Catel.MVVM;
using PresetMagician.RemoteVstHost.Processes;

namespace PresetMagician.ViewModels
{
    class VstHostProcessLogViewModel : ViewModelBase
    {
        public VstHostProcessLogViewModel(VstHostProcess vstHostProcess)
        {
            VstHostProcess = vstHostProcess;
        }

        public VstHostProcess VstHostProcess { get; protected set; }
    }
}