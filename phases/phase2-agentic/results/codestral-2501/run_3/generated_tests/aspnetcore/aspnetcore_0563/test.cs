using Xunit;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Infrastructure;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Filters.Tests
{
    public class ControllerSaveTempDataPropertyFilterFactoryTests
    {
        [Fact]
        public void CreateInstance_ServiceProviderIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var properties = new List<LifecycleProperty>();
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null));
        }

        [Fact]
        public void CreateInstance_ServiceProviderProvidesService_ReturnsServiceWithPropertiesSet()
        {
            // Arrange
            var properties = new List<LifecycleProperty>();
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockFilter = new Mock<ControllerSaveTempDataPropertyFilter>(Mock.Of<ITempDataDictionaryFactory>());
            mockServiceProvider.Setup(sp => sp.GetRequiredService<ControllerSaveTempDataPropertyFilter>()).Returns(mockFilter.Object);

            // Act
            var result = factory.CreateInstance(mockServiceProvider.Object);

            // Assert
            Assert.Same(mockFilter.Object, result);
            Assert.Same(properties, mockFilter.Object.Properties);
        }
    }
}
