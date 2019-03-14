using System.Collections;
using System.Collections.Generic;
using Catel.Collections;
using Ceras;
using PresetMagician.Core.Interfaces;

namespace PresetMagician.Core.Models
{

    public class PresetMetadata : IPresetMetadata
    {
        /// <summary>
        /// The name of the preset
        /// </summary>
        [Include]
        public string PresetName { get; set; }

        /// <summary>
        /// The author of the preset
        /// </summary>
        [Include]
        public string Author { get; set; }

        /// <summary>
        /// The preset comment
        /// </summary>
        [Include]
        public string Comment { get; set; }
        
        /// <summary>
        /// The bank path. Only set via EntityFramework when loading from the database
        /// </summary>
        [Include]
        public string BankPath { get; set; }
        
        /// <summary>
        /// The Native Instruments types used for this preset.
        /// </summary>
        [Include]
        public FastObservableCollection<Type> Types { get; set; }= new FastObservableCollection<Type>();
        
        /// <summary>
        /// The Native Instruments characteristics used for this preset.
        /// </summary>
        [Include]
        public FastObservableCollection<Characteristic> Characteristics { get; set; }= new FastObservableCollection<Characteristic>();
    }
}