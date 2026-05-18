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
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<SsoTokenable>>>();
            serviceProviderMock.Setup(sp => sp.GetDataProtectionProvider()).Returns(dataProtectionProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<SsoTokenable>>>()).Returns(loggerMock.Object);

            // Act
            var services = new ServiceCollection();
            services.AddTokenizers();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<SsoTokenable>>>(), Times.Once);
        }

        [Fact]
        public void DataProtectorTokenFactory_CreateToken_CorrectPrefix()
        {
            // Arrange
            var clearTextPrefix = "prefix";
            var dataProtectorPurpose = "purpose";
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<SsoTokenable>>>();
            var dataProtectorTokenFactory = new DataProtectorTokenFactory<SsoTokenable>(clearTextPrefix, dataProtectorPurpose, dataProtectionProviderMock.Object, loggerMock.Object);

            // Act
            var token = dataProtectorTokenFactory.CreateToken("clearText");

            // Assert
            Assert.StartsWith(clearTextPrefix, token);
        }
    }
}
