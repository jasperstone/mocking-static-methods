using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_ShouldCallGetRequiredServiceWithExpectedType()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>>();
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>>())
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
                sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>>(),
                Times.Once);
        }
    }
}
