using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Storage;
using Moq;

namespace Orleans.Persistence.AzureStorage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnAzureTableGrainStorage()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetProviderClusterOptions(It.IsAny<string>()))
                .Returns(clusterOptionsMock.Object);

            var name = "TestProvider";

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, name);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AzureTableGrainStorage>(result);
        }
    }
}
