using System.Linq;
using Catel.Collections;
using FluentAssertions;
using PresetMagician.Core.Collections;
using PresetMagician.Core.Models;
using Xunit;
using Xunit.Abstractions;

namespace PresetMagician.Tests.ModelTests
{
    public class WrappedEditableCollectionTest: BaseTest
    {
        public WrappedEditableCollectionTest(ITestOutputHelper output, DataFixture fixture) : base(output, fixture)
        {
        }

        [Fact]
        public void TestInit()
        {
            var characteristics = new EditableCollection<Characteristic>();
            characteristics.Add(new Characteristic());
            
            var wrappedCharacteristics = new WrappedEditableCollection<CharacteristicUsage, Characteristic>(characteristics);

            wrappedCharacteristics.Count.Should().Be(characteristics.Count);
            wrappedCharacteristics.First().Characteristic.Should().Be(characteristics.First());


        }
        [Fact]
        public void TestAdd()
        {
            var characteristics = new EditableCollection<Characteristic>();
            var wrappedCharacteristics = new WrappedEditableCollection<CharacteristicUsage, Characteristic>(characteristics);

            characteristics.Add(new Characteristic());
            
            wrappedCharacteristics.Count.Should().Be(characteristics.Count);
            wrappedCharacteristics.First().Characteristic.Should().Be(characteristics.First());

            var newCharacteristic = new Characteristic();
            var newWrappedCharacteristic = new CharacteristicUsage(newCharacteristic);
            wrappedCharacteristics.Add(newWrappedCharacteristic);

            wrappedCharacteristics.Count.Should().Be(2);
            wrappedCharacteristics.Count.Should().Be(characteristics.Count);
            wrappedCharacteristics.First().Characteristic.Should().Be(characteristics.First());
            wrappedCharacteristics.Last().Characteristic.Should().Be(characteristics.Last());
        }

        private void CompareList(EditableCollection<Characteristic> original,
            WrappedEditableCollection<CharacteristicUsage, Characteristic> wrapped)
        {
            wrapped.Count.Should().Be(original.Count);
            
            for (var i=0;i<wrapped.Count;i++)
            {
                wrapped[i].Characteristic.Should().Be(original[i]);

            }
        }
        
        [Fact]
        public void TestRemove()
        {
            var characteristic1 = new Characteristic() { CharacteristicName = "test1"};
            var characteristic2 = new Characteristic() {CharacteristicName = "test2"};
            var characteristic3 = new Characteristic() {CharacteristicName = "test3"};
            var characteristic4 = new Characteristic() {CharacteristicName = "test4"};
            var characteristic5 = new Characteristic() {CharacteristicName = "test5"};
            
            var characteristics = new EditableCollection<Characteristic>();
            characteristics.Add(characteristic1);
            characteristics.Add(characteristic2);
            characteristics.Add(characteristic3);
            characteristics.Add(characteristic4);
            characteristics.Add(characteristic5);
            var wrappedCharacteristics = new WrappedEditableCollection<CharacteristicUsage, Characteristic>(characteristics);

            characteristics.Count.Should().Be(5);
            wrappedCharacteristics.Count.Should().Be(characteristics.Count);
            characteristics[0].Should().Be(characteristic1);
            characteristics[1].Should().Be(characteristic2);
            characteristics[2].Should().Be(characteristic3);
            characteristics[3].Should().Be(characteristic4);
            characteristics[4].Should().Be(characteristic5);

            CompareList(characteristics, wrappedCharacteristics);
          
            wrappedCharacteristics.RemoveAt(2);
            characteristics.Count.Should().Be(4);
            CompareList(characteristics, wrappedCharacteristics);
            
            characteristics.RemoveAt(2);
            wrappedCharacteristics.Count.Should().Be(3);
            CompareList(characteristics, wrappedCharacteristics);
            
           wrappedCharacteristics.RemoveFirst();
           characteristics.Count.Should().Be(2);
           CompareList(characteristics, wrappedCharacteristics);
           
           characteristics.RemoveFirst();
           wrappedCharacteristics.Count.Should().Be(1);
           CompareList(characteristics, wrappedCharacteristics);
        }
    }
}