using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class ControllerSaveTempDataPropertyFilterFactoryTests
    {
        private class DummyFilter : ControllerSaveTempDataPropertyFilter
        {
            public bool PropertiesSet { get; private set; } = false;

            public override void SetProperties(IReadOnlyList<LifecycleProperty> properties)
            {
                Properties = properties;
                PropertiesSet = true;
            }
        }

        [Fact]
        public void CreateInstance_ReturnsServiceFromServiceProvider()
        {
            // Arrange
            var properties = new[] { new LifecycleProperty() };
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

            var dummyFilter = new DummyFilter();

            var serviceProvider = new ServiceCollection()
                .AddTransient<ControllerSaveTempDataPropertyFilter>(sp => dummyFilter)
                .BuildServiceProvider();

            // Act
            var result = factory.CreateInstance(serviceProvider);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<DummyFilter>(result);
            var filter = (DummyFilter)result;
            Assert.True(filter.PropertiesSet);
            Assert.Equal(properties, filter.Properties);
        }

        [Fact]
        public void CreateInstance_ThrowsArgumentNullException_WhenServiceProviderIsNull()
        {
            // Arrange
            var factory = new ControllerSaveTempDataPropertyFilterFactory(Array.Empty<LifecycleProperty>());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null));
        }
    }
}
