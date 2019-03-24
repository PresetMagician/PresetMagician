using System.Collections.Generic;
using System.Linq;
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
        public FastObservableCollection<Type> Types { get; set; } = new FastObservableCollection<Type>();

        /// <summary>
        /// The Native Instruments characteristics used for this preset.
        /// </summary>
        public FastObservableCollection<Characteristic> Characteristics { get; set; } =
            new FastObservableCollection<Characteristic>();
        
        [Include]
        public List<Type> SerializedTypes { get; set; }
        
        [Include]
        public List<Characteristic> SerializedCharacteristics { get; set; }
        
        public void OnBeforeCerasSerialize()
        {
            SerializedTypes = Types.ToList();
            SerializedCharacteristics = Characteristics.ToList();
        }
        
        public void OnAfterCerasDeserialize()
        {
            using (Types.SuspendChangeNotifications())
            {
                Types.Clear();
                Types.AddItems(SerializedTypes);
            }
            
            using (Characteristics.SuspendChangeNotifications())
            {
                Characteristics.Clear();
                Characteristics.AddItems(SerializedCharacteristics);
            }
        }
    }
}