using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Filters.Tests
{
    public class ControllerSaveTempDataPropertyFilterFactoryTests
    {
        [Fact]
        public void CreateInstance_ThrowsArgumentNullException_WhenServiceProviderIsNull()
        {
            // Arrange
            var factory = new ControllerSaveTempDataPropertyFilterFactory(Array.Empty<LifecycleProperty>());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null));
        }

        [Fact]
        public void CreateInstance_ReturnsFilter_WhenServiceProviderHasRequiredService()
        {
            // Arrange
            var tempDataFactory = Mock.Of<ITempDataDictionaryFactory>();
            var filter = new ControllerSaveTempDataPropertyFilter(tempDataFactory);
            
            var services = new ServiceCollection();
            services.AddSingleton(filter);
            var serviceProvider = services.BuildServiceProvider();

            var properties = Array.Empty<LifecycleProperty>();
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

            // Act
            var result = factory.CreateInstance(serviceProvider);

            // Assert
            Assert.Same(filter, result);
            Assert.Same(properties, ((ControllerSaveTempDataPropertyFilter)result).Properties);
        }

        [Fact]
        public void CreateInstance_ThrowsInvalidOperationException_WhenServiceProviderMissingRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            var factory = new ControllerSaveTempDataPropertyFilterFactory(Array.Empty<LifecycleProperty>());

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateInstance(serviceProvider));
            Assert.Contains("ControllerSaveTempDataPropertyFilter", exception.Message);
        }

        [Fact]
        public void IsReusable_ReturnsFalse()
        {
            // Arrange
            var factory = new ControllerSaveTempDataPropertyFilterFactory(Array.Empty<LifecycleProperty>());

            // Act & Assert
            Assert.False(factory.IsReusable);
        }
    }
}
