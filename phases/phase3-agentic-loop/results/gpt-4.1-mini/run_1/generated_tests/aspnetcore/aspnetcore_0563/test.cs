using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Filters
{
    public class ControllerSaveTempDataPropertyFilterFactoryTests
    {
        [Fact]
        public void CreateInstance_ThrowsArgumentNullException_WhenServiceProviderIsNull()
        {
            // Arrange
            var factory = new ControllerSaveTempDataPropertyFilterFactory(new List<LifecycleProperty>());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null!));
        }

        [Fact]
        public void CreateInstance_ReturnsFilterWithPropertiesSet()
        {
            // Arrange
            var properties = new List<LifecycleProperty>
            {
                new LifecycleProperty(typeof(TestController).GetProperty(nameof(TestController.Prop1))!, "key1"),
                new LifecycleProperty(typeof(TestController).GetProperty(nameof(TestController.Prop2))!, "key2")
            };
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

            var filter = new ControllerSaveTempDataPropertyFilter(Mock.Of<ITempDataDictionaryFactory>());

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(ControllerSaveTempDataPropertyFilter)))
                .Returns(filter);

            // Act
            var result = factory.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.Same(filter, result);
            Assert.Equal(properties, filter.Properties);
        }

        private class TestController
        {
            public string Prop1 { get; set; } = string.Empty;
            public int Prop2 { get; set; }
        }
    }
}
