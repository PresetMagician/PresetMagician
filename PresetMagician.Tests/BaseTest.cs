using System;
using Xunit;
using Xunit.Abstractions;

namespace PresetMagician.Tests
{
    public class BaseTest : IClassFixture<DataFixture>, IDisposable
    {
        protected readonly ITestOutputHelper _output;
        protected DataFixture Fixture;

        public BaseTest(ITestOutputHelper output, DataFixture fixture)
        {
            _output = output;
            Fixture = fixture;


            fixture.Setup(GetType().Name);
        }

        public void Dispose()
        {
        }
    }
}