using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Filters.Tests
{
    public class ControllerSaveTempDataPropertyFilterFactoryTests
    {
        private class DummyFilter : IFilterMetadata
        {
            public IReadOnlyList<LifecycleProperty> Properties { get; set; }
        }

        [Fact]
        public void CreateInstance_ShouldCallGetRequiredServiceAndSetProperties()
        {
            // Arrange
            var properties = new[] { new LifecycleProperty() };
            var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

            var mockFilter = new DummyFilter();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ControllerSaveTempDataPropertyFilter>())
                .Returns(mockFilter);

            // Act
            var result = factory.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.Equal(mockFilter, result);
            Assert.Equal(properties, ((DummyFilter)result).Properties);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ControllerSaveTempDataPropertyFilter>(), Times.Once);
        }

        [Fact]
        public void CreateInstance_ShouldThrowArgumentNullException_WhenServiceProviderIsNull()
        {
            // Arrange
            var factory = new ControllerSaveTempDataPropertyFilterFactory(Array.Empty<LifecycleProperty>());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null));
        }
    }
}
