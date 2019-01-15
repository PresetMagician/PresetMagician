using System.Collections.ObjectModel;
using PresetMagician.Models;
using SharedModels;

namespace Drachenkatze.PresetMagician.VSTHost.VST
{
    public interface IVstHost
    {
        /// <summary>
        ///     Returns all found DLLs for a specific directory
        /// </summary>
        /// <param name="pluginDirectory"></param>
        /// <returns></returns>
        ObservableCollection<string> EnumeratePlugins(string pluginDirectory);

        void LoadVST(Plugin vst);
        void MIDI_CC(Plugin plugin, byte Number, byte Value);
        void MIDI_NoteOff(Plugin plugin, byte Note, byte Velocity);
        void MIDI_NoteOn(Plugin plugin, byte Note, byte Velocity);
        void UnloadVST(Plugin vst);
    }
}