using Catel.Reflection;

namespace PresetMagician.Tests
{
    public class ModuleInitializer
    {
        public static void Initialize()
        {
            TypeCache.InitializeTypes();
        }
    }
}