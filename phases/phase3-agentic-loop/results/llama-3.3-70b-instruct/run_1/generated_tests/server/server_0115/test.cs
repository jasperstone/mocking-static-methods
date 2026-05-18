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
            var loggerMock = new Mock<ILogger>();

            serviceProviderMock.Setup(sp => sp.GetDataProtectionProvider()).Returns(dataProtectionProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILogger))).Returns(loggerMock.Object);

            var services = new ServiceCollection();
            services.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            ServiceCollectionExtensions.AddTokenizers(services);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILogger)), Times.AtLeastOnce);
        }
    }
}
