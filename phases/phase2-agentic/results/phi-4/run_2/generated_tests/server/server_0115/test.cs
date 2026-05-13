using System;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDataProtectorTokenFactories_CallsGetRequiredServiceWithCorrectType()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>();
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>())
                .Returns(loggerMock.Object);

            var services = new ServiceCollection();
            var extensions = new ServiceCollectionExtensions();

            // Act
            extensions.AddDataProtectorTokenFactories(services, serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(
                sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>(),
                Times.Once);
        }
    }
}
