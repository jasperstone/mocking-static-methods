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
        public void Should_Call_GetRequiredService_For_ILogger()
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
            serviceProviderMock.Verify(sp => sp.GetRequiredService<ILogger<DataProtectorTokenFactory<DuoUserStateTokenable>>>(), Times.Once);
        }
    }
}
