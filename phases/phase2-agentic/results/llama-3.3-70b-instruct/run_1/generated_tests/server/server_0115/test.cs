using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_GetRequiredService_Called()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>();
            serviceProviderMock.Setup(s => s.GetDataProtectionProvider()).Returns(dataProtectionProviderMock.Object);
            serviceProviderMock.Setup(s => s.GetRequiredService<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>()).Returns(loggerMock.Object);

            // Act
            var services = new ServiceCollection();
            services.AddTokenizers();

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>(), Times.Once);
        }
    }
}
