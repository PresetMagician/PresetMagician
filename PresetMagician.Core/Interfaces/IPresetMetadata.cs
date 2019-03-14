using System.Collections.Generic;
using Catel.Collections;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Interfaces
{
    public interface IPresetMetadata
    {
        /// <summary>
        /// The name of the preset
        /// </summary>
        string PresetName { get; set; }

        /// <summary>
        /// The author of the preset
        /// </summary>
        string Author { get; set; }

        /// <summary>
        /// The preset comment
        /// </summary>
        string Comment { get; set; }

        /// <summary>
        /// The bank path. Only set via EntityFramework when loading from the database
        /// </summary>
        string BankPath { get; set; }

        /// <summary>
        /// The Native Instruments types used for this preset.
        /// </summary>
        FastObservableCollection<Type> Types { get; set; }

        /// <summary>
        /// The Native Instruments characteristics used for this preset.
        /// </summary>
        FastObservableCollection<Characteristic> Characteristics { get; set; }

    }
}