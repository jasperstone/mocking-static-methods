using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Persistence.AzureStorage.Providers.Storage;
using Orleans;
using System;

namespace Orleans.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_Should_Call_GetRequiredService_ForOptionsMonitor()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();

            // Setup the IServiceProvider to return the options monitor when requested
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(IOptionsMonitor<AzureTableStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, "TestProvider");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IOptionsMonitor<AzureTableStorageOptions>)), Times.Once);
        }
    }
}
