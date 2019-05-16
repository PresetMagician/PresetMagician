using System.Collections.Generic;
using PresetMagician.Core.Models.MIDI;
using RtMidi.Core;
using RtMidi.Core.Devices;

namespace PresetMagician.Core.Services
{
    public class MidiService
    {
        private static List<MidiInputDevice> _midiDevices;

        public MidiService()
        {
            if (_midiDevices == null)
            {
                _midiDevices = new List<MidiInputDevice>();

                foreach (var od in MidiDeviceManager.Default.OutputDevices)
                {
                    _midiDevices.Add(new MidiInputDevice() {Name = od.Name});
                }
            }
        }

        public List<MidiInputDevice> GetInputDevices()
        {
            return _midiDevices;
        }

        public IMidiInputDevice GetMidiEndpoint(MidiInputDevice device)
        {
            foreach (var inputDevice in MidiDeviceManager.Default.InputDevices)
            {
                if (inputDevice.Name == device.Name)
                {
                    return inputDevice.CreateDevice();
                }
            }

            return null;
        }
    }
}