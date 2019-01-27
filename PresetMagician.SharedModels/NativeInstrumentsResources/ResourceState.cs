using PresetMagician.Models.NativeInstrumentsResources;

namespace SharedModels.NativeInstrumentsResources
{
    public class ResourceState
    {
        private NativeInstrumentsResource.ResourceStates
            _resourceState = NativeInstrumentsResource.ResourceStates.Empty;

        public NativeInstrumentsResource.ResourceStates State
        {
            get => _resourceState;
            set
            {
                switch (value)
                {
                    case NativeInstrumentsResource.ResourceStates.Empty:
                    case NativeInstrumentsResource.ResourceStates.FromDisk:
                        ShouldSave = false;
                        break;
                    case NativeInstrumentsResource.ResourceStates.FromWeb:
                    case NativeInstrumentsResource.ResourceStates.UserModified:
                    case NativeInstrumentsResource.ResourceStates.AutomaticallyGenerated:
                        ShouldSave = true;
                        break;
                }

                _resourceState = value;
            }
        }

        public bool ShouldSave { get; set; }
    }
}