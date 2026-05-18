using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceProviderExtensionsTests
    {
        [Fact]
        public void GetRequiredService_CallsGetServiceOnIServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();

            var expectedService = new object();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(object)))
                .Returns(expectedService);

            // Act
            var actualService = serviceProviderMock.Object.GetRequiredService<object>();

            // Assert
            Assert.Same(expectedService, actualService);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(object)), Times.Once);
        }
    }
}
