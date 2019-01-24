using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PresetMagician.Models;
using PresetMagician.VstHost.VST;
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

        bool LoadVst(VstPlugin vst);
        void MIDI_CC(VstPlugin plugin, byte Number, byte Value);
        void MIDI_NoteOff(VstPlugin plugin, byte Note, byte Velocity);
        void MIDI_NoteOn(VstPlugin plugin, byte Note, byte Velocity);
        void UnloadVst(VstPlugin vst);
    }
}