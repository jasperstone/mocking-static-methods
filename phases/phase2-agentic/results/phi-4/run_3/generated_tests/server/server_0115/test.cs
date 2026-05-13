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
        public void AddTokenizers_CallsGetRequiredServiceWithCorrectType()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>();
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>())
                .Returns(loggerMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetDataProtectionProvider())
                .Returns(dataProtectionProviderMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton(serviceProviderMock.Object);

            // Act
            ServiceCollectionExtensions.AddTokenizers(services);

            // Assert
            serviceProviderMock.Verify(
                sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>(),
                Times.Once);
        }
    }
}
