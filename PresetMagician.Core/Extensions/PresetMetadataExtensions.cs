using PresetMagician.Core.Interfaces;

namespace PresetMagician.Core.Extensions
{
    public static class PresetMetadataExtensions
    {
        public static bool IsEqualTo<T>(this T metadata, IPresetMetadata compare, string propertyName) where T : IPresetMetadata
        {
            switch (propertyName)
            {
                case nameof(IPresetMetadata.PresetName):
                    return metadata.PresetName == compare.PresetName;
                case nameof(IPresetMetadata.Author):
                    return metadata.Author == compare.Author;
                case nameof(IPresetMetadata.Comment):
                    return metadata.Comment == compare.Comment;
                case nameof(IPresetMetadata.BankPath):
                    return metadata.BankPath == compare.BankPath;
                case nameof(IPresetMetadata.Types):
                    return metadata.Types.IsEqualTo(compare.Types);
                case nameof(IPresetMetadata.Characteristics):
                    return metadata.Characteristics.IsEqualTo(compare.Characteristics);
            }

            return false;
        }

        public static bool IsEqualTo<T>(this T metadata, IPresetMetadata compare) where T: IPresetMetadata
        {
            if (metadata.PresetName != compare.PresetName)
            {
                return false;
            }
            
            if (metadata.Author != compare.Author)
            {
                return false;
            }
            
            if (metadata.Comment != compare.Comment)
            {
                return false;
            }
            
            if (metadata.BankPath != compare.BankPath)
            {
                return false;
            }

            if (!metadata.Characteristics.IsEqualTo(compare.Characteristics, true))
            {
                return false;
            }

            if (!metadata.Types.IsEqualTo(compare.Types, true))
            {
                return false;
            }
            return true;
        }
    }
}