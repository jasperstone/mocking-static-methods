using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Filters
{
    public class ControllerSaveTempDataPropertyFilterFactoryTest
    {
        [Fact]
        public void CreateInstance_ThrowsArgumentNullException_WhenServiceProviderIsNull()
        {
            // Arrange
            var properties = new List<LifecycleProperty>();
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null));
        }

        [Fact]
        public void CreateInstance_ReturnsFilterWithPropertiesSet()
        {
            // Arrange
            var properties = new List<LifecycleProperty>
            {
                new LifecycleProperty(typeof(TestController).GetProperty(nameof(TestController.TempDataProperty)), "TempDataProperty")
            };
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

            var filter = new ControllerSaveTempDataPropertyFilter(Mock.Of<ITempDataDictionaryFactory>());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ControllerSaveTempDataPropertyFilter>())
                .Returns(filter);

            // Act
            var result = factory.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.Same(filter, result);
            Assert.Equal(properties, filter.Properties);
        }

        private class TestController
        {
            [TempData]
            public string TempDataProperty { get; set; }
        }
    }
}
