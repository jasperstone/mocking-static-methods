using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Persistence.AzureStorage;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.AzureStorage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldCallGetRequiredServiceForIOptionsMonitorAndReturnAzureTableGrainStorage()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            serviceProviderMock
                .Setup(s => s.GetService(typeof(IOptions<ClusterOptions>)))
                .Returns(clusterOptionsMock.Object);

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, "TestProvider");

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>(), Times.Once);
            Assert.IsType<AzureTableGrainStorage>(result);
        }
    }
}
