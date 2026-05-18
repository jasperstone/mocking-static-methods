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
        public void AddTokenizers_ShouldRegisterDataProtectorTokenFactories()
        {
            // Arrange
            var services = new ServiceCollection();

            // Mock the ILogger
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>>();
            var loggerProviderMock = new Mock<ILoggerProvider>();
            loggerProviderMock
                .Setup(p => p.CreateLogger(It.IsAny<string>()))
                .Returns(loggerMock.Object);

            services.AddSingleton<ILoggerProvider>(loggerProviderMock.Object);

            // Mock the IDataProtectionProvider
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            services.AddSingleton<IDataProtectionProvider>(dataProtectionProviderMock.Object);

            // Act
            ServiceCollectionExtensions.AddTokenizers(services);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var factory = serviceProvider.GetRequiredService<IDataProtectorTokenFactory<WebAuthnCredentialCreateOptionsTokenable>>();

            Assert.NotNull(factory);
            loggerMock.Verify(l => l.IsEnabled(It.IsAny<LogLevel>()), Times.Once);
        }
    }
}
