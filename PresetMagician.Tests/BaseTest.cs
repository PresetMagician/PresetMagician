using Drachenkatze.PresetMagician.VendorPresetParser;
using Xunit;
using Xunit.Abstractions;

namespace PresetMagician.Tests
{
    public class BaseTest:IClassFixture<DataFixture>
    {
        protected readonly ITestOutputHelper _output;
        private DataFixture _fixture;
        
        public BaseTest(ITestOutputHelper output, DataFixture fixture)
        {
            _output = output;
            _fixture = fixture;
            
            Core.CoreInitializer.RegisterServices();
            VendorPresetParserInitializer.Initialize();
            fixture.Setup(GetType().Name);
        }
    }
}