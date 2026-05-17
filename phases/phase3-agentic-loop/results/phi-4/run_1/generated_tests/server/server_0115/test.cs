using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DataProtection;
using Moq;
using Xunit;
using Bit.Core.Utilities; // Ensure this namespace is correct for DataProtectorTokenFactory
using Bit.Core.Models; // Assuming DuoUserStateTokenable is in this namespace

namespace SharedWeb.Utilities.Tests
{
    public class ServiceProviderServiceExtensionsTests
    {
        [Fact]
        public void Test_GetRequiredService_CallsWithCorrectType()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>();
            var dataProtectionProviderMock = new Mock<IDataProtectionProvider>();

            // Set up the mock to return the logger and data protection provider when GetRequiredService is called with the correct types
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>())
                .Returns(loggerMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetDataProtectionProvider())
                .Returns(dataProtectionProviderMock.Object);

            // Act
            var services = new ServiceCollection();
            services.AddSingleton<IDataProtectorTokenFactory<DuoUserStateTokenable>>(serviceProvider =>
                new DataProtectorTokenFactory<DuoUserStateTokenable>(
                    DuoUserStateTokenable.ClearTextPrefix,
                    DuoUserStateTokenable.DataProtectorPurpose,
                    serviceProvider.GetDataProtectionProvider(),
                    serviceProvider.GetRequiredService<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>())
            );

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IDataProtectorTokenFactory<DuoUserStateTokenable>>();

            // Assert
            serviceProviderMock.Verify(
                sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>(),
                Times.Once);

            serviceProviderMock.Verify(
                sp => sp.GetDataProtectionProvider(),
                Times.Once);
        }
    }
}
