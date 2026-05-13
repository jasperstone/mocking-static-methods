using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RequestFormLimitsAttributeTests
    {
        [Fact]
        public void CreateInstance_ServiceProvider_GetRequiredServiceCalled()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var filterMock = new Mock<RequestFormLimitsFilter>();
            serviceProviderMock.Setup(p => p.GetRequiredService<RequestFormLimitsFilter>()).Returns(filterMock.Object);

            var attribute = new RequestFormLimitsAttribute();

            // Act
            var filterMetadata = attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<RequestFormLimitsFilter>(), Times.Once);
            Assert.Same(filterMock.Object, filterMetadata);
        }

        [Fact]
        public void CreateInstance_ServiceProvider_FilterFormOptionsSet()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var filterMock = new Mock<RequestFormLimitsFilter>();
            serviceProviderMock.Setup(p => p.GetRequiredService<RequestFormLimitsFilter>()).Returns(filterMock.Object);

            var attribute = new RequestFormLimitsAttribute();

            // Act
            var filterMetadata = attribute.CreateInstance(serviceProviderMock.Object);

            // Assert
            Assert.Same(attribute.FormOptions, filterMock.Object.FormOptions);
        }
    }
}
