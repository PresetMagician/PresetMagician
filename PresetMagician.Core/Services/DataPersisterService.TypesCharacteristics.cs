using System.IO;
using Catel.Collections;
using Ceras;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Models;

namespace PresetMagician.Core.Services
{
    public partial class DataPersisterService
    {
        private static string GetTypesStorageFile()
        {
            Directory.CreateDirectory(DefaultDataStoragePath);
            return Path.Combine(DefaultDataStoragePath, TypesStorageFile);
        }

        private static string GetCharacteristicsStorageFile()
        {
            Directory.CreateDirectory(DefaultDataStoragePath);
            return Path.Combine(DefaultDataStoragePath, CharacteristicsStorageFile);
        }

        public void SaveTypesCharacteristics()
        {
            var serializer = GetSaveSerializer();


            var typesDataFile = GetTypesStorageFile();
            var typesData = serializer.Serialize(_globalService.GlobalTypes);

            File.WriteAllBytes(typesDataFile, typesData);

            var characteristicsDataFile = GetCharacteristicsStorageFile();
            var characteristicsData = serializer.Serialize(_globalService.GlobalCharacteristics);

            File.WriteAllBytes(characteristicsDataFile, characteristicsData);
        }

        public void LoadTypesCharacteristics()
        {
            var typesDataFile = GetTypesStorageFile();

            if (File.Exists(typesDataFile))
            {
                var types = GetLoadSerializer().Deserialize<EditableCollection<Type>>(File.ReadAllBytes(typesDataFile));
                _globalService.GlobalTypes.Clear();
                _globalService.GlobalTypes.AddRange(types);
            }

            var characteristicsDataFile = GetCharacteristicsStorageFile();

            if (File.Exists(characteristicsDataFile))
            {
                var characteristics =
                    GetLoadSerializer().Deserialize<EditableCollection<Characteristic>>(
                        File.ReadAllBytes(characteristicsDataFile));
                _globalService.GlobalCharacteristics.Clear();
                _globalService.GlobalCharacteristics.AddRange(characteristics);
            }
        }
    }
}