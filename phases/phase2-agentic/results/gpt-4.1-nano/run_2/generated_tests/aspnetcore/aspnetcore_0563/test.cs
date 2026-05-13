using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Filters.Tests
{
    public class ControllerSaveTempDataPropertyFilterFactoryTests
    {
        [Fact]
        public void CreateInstance_ShouldRetrieveServiceAndSetProperties()
        {
            // Arrange
            var tempDataProperties = new[] { new LifecycleProperty() };
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
            Assert.Equal(mockFilter.Object, result);
            Assert.Equal(tempDataProperties, mockFilter.Object.Properties);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ControllerSaveTempDataPropertyFilter>(), Times.Once);
        }
    }
}
