using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Storage;

namespace Orleans.Persistence.AzureStorage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnAzureTableGrainStorage()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptions<ClusterOptions>)))
                .Returns(clusterOptionsMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOptions<ClusterOptions>)))
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
