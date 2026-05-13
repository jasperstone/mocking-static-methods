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
        public void CreateInstance_ServiceProviderIsValid_ReturnsFilter()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<ControllerSaveTempDataPropertyFilter>()
                .BuildServiceProvider();

            var factory = new ControllerSaveTempDataPropertyFilterFactory(new List<LifecycleProperty>());

            // Act
            var filter = factory.CreateInstance(serviceProvider);

            // Assert
            Assert.NotNull(filter);
            Assert.IsType<ControllerSaveTempDataPropertyFilter>(filter);
        }

        [Fact]
        public void CreateInstance_ServiceProviderIsValid_SetsProperties()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddScoped<ControllerSaveTempDataPropertyFilter>()
                .BuildServiceProvider();

            var properties = new List<LifecycleProperty>();
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

            // Act
            var filter = factory.CreateInstance(serviceProvider);

            // Assert
            Assert.Same(properties, ((ControllerSaveTempDataPropertyFilter)filter).Properties);
        }

        [Fact]
        public void CreateInstance_ServiceProviderDoesNotContainFilter_ThrowsException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var factory = new ControllerSaveTempDataPropertyFilterFactory(new List<LifecycleProperty>());

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => factory.CreateInstance(serviceProvider));
        }
    }
}
