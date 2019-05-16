using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using PresetMagician.Core.Models.Audio;

namespace PresetMagician.Core.Services
{
    public class AudioService
    {
        private MMDeviceEnumerator _enumerator;
        private static List<AudioOutputDevice> _audioDevices;

        public AudioService()
        {
            _enumerator = new MMDeviceEnumerator();

            if (_audioDevices == null)
            {
                _audioDevices = new List<AudioOutputDevice>();


                _audioDevices.Add(CreateDefaultAudioDevice());

                foreach (var wasapi in _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                {
                    _audioDevices.Add(new AudioOutputDevice(wasapi.DeviceFriendlyName, wasapi.ID,
                        AudioDeviceType.AUDIODEVICETYPE_WASAPI));
                }
            }
        }

        public List<AudioOutputDevice> GetOutputDevices()
        {
            return _audioDevices;
        }

        public MMDevice GetAudioEndpoint(AudioOutputDevice device)
        {
            if (device.DefaultDevice)
            {
                return _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }

            foreach (var dev in _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                if (dev.ID == device.Id)
                {
                    return dev;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the actual audio device from a serialized AudioDevice object.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public AudioOutputDevice GetDefaultAudioDevice()
        {
            return (from dev in _audioDevices where dev.DefaultDevice select dev).First();
        }

        private AudioOutputDevice CreateDefaultAudioDevice()
        {
            var defaultAudioEndpoint = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return new AudioOutputDevice($"Default ({defaultAudioEndpoint.DeviceFriendlyName})",
                defaultAudioEndpoint.ID,
                AudioDeviceType.AUDIODEVICETYPE_WASAPI, true);
        }
    }
}