using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Tests
{
    public class ControllerSaveTempDataPropertyFilterFactoryTests
    {
        [Fact]
        public void CreateInstance_GetRequiredServiceIsCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var tempDataDictionaryFactoryMock = new Mock<ITempDataDictionaryFactory>();
            var filterMock = new Mock<ControllerSaveTempDataPropertyFilter>(tempDataDictionaryFactoryMock.Object);
            serviceProviderMock.Setup(p => p.GetRequiredService<ControllerSaveTempDataPropertyFilter>()).Returns(filterMock.Object);
            var properties = new List<LifecycleProperty>();
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

            // Act
            var result = factory.CreateInstance(serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<ControllerSaveTempDataPropertyFilter>(), Times.Once);
            Assert.Same(filterMock.Object, result);
            Assert.Same(properties, ((ControllerSaveTempDataPropertyFilter)result).Properties);
        }

        [Fact]
        public void CreateInstance_ThrowsArgumentNullExceptionWhenServiceProviderIsNull()
        {
            // Arrange
            var properties = new List<LifecycleProperty>();
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null));
        }
    }
}
