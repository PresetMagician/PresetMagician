using System.Diagnostics;

namespace PresetMagician.Tests
{
    public class ModuleInitializer
    {
      
            public static void Initialize()
            {
                Catel.Reflection.TypeCache.InitializeTypes();
               Debug.WriteLine("init");
            }
    }
}