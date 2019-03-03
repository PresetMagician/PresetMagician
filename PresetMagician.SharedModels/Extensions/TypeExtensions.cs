using SharedModels.Collections;

namespace SharedModels.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsTrackableCollection(this System.Type candidate)
        {
            if (null == candidate) return false;

            bool itIs = candidate.IsGenericType &&
                        !candidate.IsGenericTypeDefinition &&
                        candidate.GetGenericTypeDefinition() == typeof(TrackableCollection<>);

            return itIs;
        }
        
        public static System.Type GetTrackableCollectionType(this System.Type candidate) {
            bool isObservableCollection = IsTrackableCollection(candidate);
            if (!isObservableCollection) return null;

            var elementType = candidate.GetType().GetGenericArguments()[0];
            return elementType;
        }
    }
}