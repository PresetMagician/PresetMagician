using System.Collections.Generic;
using System.Linq;
using Catel.Collections;
using Catel.Reflection;
using FluentAssertions;

namespace PresetMagician.Tests.Helpers
{
    public class PropertyComparisonHelper
    {
        private object Source;
        private object Target;

        private HashSet<string> VisitedProperties = new HashSet<string>();
        private HashSet<string> AllProperties = new HashSet<string>();

        public PropertyComparisonHelper(object source, object target)
        {
            Source = source;
            Target = target;

            var sourceProperties =
                (from prop in Source.GetType().GetProperties() select prop.Name).ToList();
            var targetProperties =
                (from prop in Target.GetType().GetProperties() select prop.Name).ToList();

            AllProperties.AddRange(sourceProperties.Intersect(targetProperties));
        }

        public HashSet<string> GetAllProperties()
        {
            return AllProperties;
        }

        public void AddVisitedProperty(string name)
        {
            VisitedProperties.Add(name);
        }

        public void CompareProperties(HashSet<string> propertiesToCompare)
        {
            foreach (var propertyName in propertiesToCompare)
            {
                var sourceValue = PropertyHelper.GetPropertyValue(Source, propertyName);
                var targetValue = PropertyHelper.GetPropertyValue(Target, propertyName);

                targetValue.Should()
                    .BeEquivalentTo(sourceValue, $"Property {propertyName} should be equal for both objects");
                VisitedProperties.Add(propertyName);
            }
        }

        public List<string> GetUnvisitedProperties()
        {
            return AllProperties.Except(VisitedProperties).ToList();
        }
    }
}