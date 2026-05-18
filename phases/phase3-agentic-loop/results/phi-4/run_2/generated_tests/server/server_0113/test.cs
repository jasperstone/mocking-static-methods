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
        public void AddTokenizers_ShouldCallGetRequiredServiceWithExpectedTypes()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Mock the GetRequiredService method for ILogger
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>>())
                .Returns(Mock.Of<ILogger<DataProtectorTokenFactory<SsoEmail2faSessionTokenable>>>());

            // Mock the GetDataProtectionProvider method
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IDataProtectionProvider)))
                .Returns(Mock.Of<IDataProtectionProvider>());

            // Set the mock as the service provider
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
