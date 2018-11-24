using Drachenkatze.PresetMagician.VSTHost.VST;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Drachenkatze.PresetMagician.VendorPresetParserTests
{
    [TestClass]
    public class VendorPresetParserTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var vstPlugin = new VSTPlugin(@"C:\Users\Drachenkatze\Documents\SafeVSTPlugins\Kairatune.dll");
            var vstHost = new VstHost();

            var plugin = vstHost.LoadVST(vstPlugin);

            var handler = VendorPresetParser.VendorPresetParser.GetPresetHandler(vstPlugin);
            
        }
    }
}
