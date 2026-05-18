using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Reflection;
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
            var properties = new List<LifecycleProperty>();
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            // Act
            var filter = factory.CreateInstance(serviceProvider);

            // Assert
            Assert.NotNull(filter);
            Assert.IsType<ControllerSaveTempDataPropertyFilter>(filter);
        }

        [Fact]
        public void CreateInstance_ServiceProviderHasFilter_ReturnsFilterWithProperties()
        {
            // Arrange
            var propertyInfo = typeof(string).GetProperty("Length");
            var properties = new List<LifecycleProperty> { new LifecycleProperty(propertyInfo, "Length") };
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);
            var serviceProviderMock = new Mock<IServiceProvider>();
            var filterMock = new Mock<ControllerSaveTempDataPropertyFilter>();
            serviceProviderMock.Setup(p => p.GetRequiredService<ControllerSaveTempDataPropertyFilter>()).Returns(filterMock.Object);

            // Act
            var filter = factory.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.NotNull(filter);
            Assert.IsType<ControllerSaveTempDataPropertyFilter>(filter);
            Assert.Same(properties, ((ControllerSaveTempDataPropertyFilter)filter).Properties);
        }
    }
}
