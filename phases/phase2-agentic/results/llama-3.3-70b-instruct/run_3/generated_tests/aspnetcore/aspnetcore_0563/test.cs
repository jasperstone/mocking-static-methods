using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class ControllerSaveTempDataPropertyFilterFactoryTests
    {
        [Fact]
        public void CreateInstance_GetRequiredServiceIsCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var filterMock = new Mock<ControllerSaveTempDataPropertyFilter>();
            serviceProviderMock.Setup(p => p.GetRequiredService<ControllerSaveTempDataPropertyFilter>()).Returns(filterMock.Object);

            var factory = new ControllerSaveTempDataPropertyFilterFactory(new List<LifecycleProperty>());

            // Act
            var filter = (ControllerSaveTempDataPropertyFilter)factory.CreateInstance(serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<ControllerSaveTempDataPropertyFilter>(), Times.Once);
            Assert.Same(filterMock.Object, filter);
            Assert.Same(factory.TempDataProperties, filter.Properties);
        }

        [Fact]
        public void CreateInstance_ThrowsArgumentNullException()
        {
            // Arrange
            var factory = new ControllerSaveTempDataPropertyFilterFactory(new List<LifecycleProperty>());

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null));
        }
    }
}
