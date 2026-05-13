using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class ControllerSaveTempDataPropertyFilterFactoryTests
    {
        [Fact]
        public void CreateInstance_ServiceProviderIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var factory = new ControllerSaveTempDataPropertyFilterFactory(new List<LifecycleProperty>());

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null));
        }

        [Fact]
        public void CreateInstance_ServiceProviderIsNotNull_ReturnsFilter()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var factory = new ControllerSaveTempDataPropertyFilterFactory(new List<LifecycleProperty>());

            // Act
            var filter = factory.CreateInstance(serviceProvider);

            // Assert
            Assert.NotNull(filter);
        }

        [Fact]
        public void CreateInstance_ServiceProviderHasRequiredService_ReturnsFilterWithProperties()
        {
            // Arrange
            var properties = new List<LifecycleProperty>();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<ControllerSaveTempDataPropertyFilter>(new ControllerSaveTempDataPropertyFilter(Mock.Of<ITempDataDictionaryFactory>()))
                .BuildServiceProvider();
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

            // Act
            var filter = (ControllerSaveTempDataPropertyFilter)factory.CreateInstance(serviceProvider);

            // Assert
            Assert.NotNull(filter);
            Assert.Same(properties, filter.Properties);
        }
    }
}
