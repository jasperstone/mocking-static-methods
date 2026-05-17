using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;

namespace MyNamespace.Tests
{
    public class ControllerSaveTempDataPropertyFilterFactoryTests
    {
        [Fact]
        public void CreateInstance_ShouldRetrieveServiceAndSetProperties()
        {
            // Arrange
            var tempDataProperties = new List<LifecycleProperty> { new LifecycleProperty() };
            var factory = new ControllerSaveTempDataPropertyFilterFactory(tempDataProperties);

            var mockFilter = new Mock<ControllerSaveTempDataPropertyFilter>();
            mockFilter.SetupProperty(f => f.Properties);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ControllerSaveTempDataPropertyFilter>())
                .Returns(mockFilter.Object);

            // Act
            var result = factory.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.Same(mockFilter.Object, result);
            Assert.Equal(tempDataProperties, mockFilter.Object.Properties);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ControllerSaveTempDataPropertyFilter>(), Times.Once);
        }

        [Fact]
        public void CreateInstance_ShouldThrowArgumentNullException_WhenServiceProviderIsNull()
        {
            // Arrange
            var factory = new ControllerSaveTempDataPropertyFilterFactory(new List<LifecycleProperty>());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null));
        }
    }
}
