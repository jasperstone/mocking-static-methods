using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Tests
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
            var serviceProviderMock = new Mock<IServiceProvider>();
            var filterMock = new Mock<ControllerSaveTempDataPropertyFilter>(MockBehavior.Strict, new Mock<ITempDataDictionaryFactory>().Object);
            serviceProviderMock.Setup(p => p.GetRequiredService<ControllerSaveTempDataPropertyFilter>()).Returns(filterMock.Object);

            // Act
            var filter = factory.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.NotNull(filter);
            Assert.Same(filterMock.Object, filter);
            Assert.Same(properties, ((SaveTempDataPropertyFilterBase)filter).Properties);
        }
    }
}
