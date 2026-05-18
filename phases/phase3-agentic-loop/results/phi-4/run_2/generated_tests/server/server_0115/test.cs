using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_ShouldCallGetRequiredServiceWithCorrectType()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>();

            // Setup the mock to return the logger when GetRequiredService is called
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>())
                .Returns(loggerMock.Object);

            // Act
            services.AddSingleton(serviceProviderMock.Object); // Add the mock to the service collection
            ServiceCollectionExtensions.AddTokenizers(services);

            // Assert
            serviceProviderMock.Verify(
                sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>(),
                Times.Once);
        }
    }
}
