using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Storage;
using Orleans.Runtime;
using Orleans.Persistence.AzureStorage;

namespace Orleans.Tests.Storage
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnAzureTableGrainStorageInstance()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            var activatorProviderMock = new Mock<IActivatorProvider>();

            var options = new AzureTableStorageOptions();
            var clusterOptions = new ClusterOptions();

            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);
            serviceProviderMock.Setup(m => m.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>()).Returns(optionsMonitorMock.Object);
            serviceProviderMock.Setup(m => m.GetRequiredService<IOptions<ClusterOptions>>()).Returns(clusterOptionsMock.Object);
            serviceProviderMock.Setup(m => m.GetRequiredService<IActivatorProvider>()).Returns(activatorProviderMock.Object);

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, "TestName");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AzureTableGrainStorage>(result);
        }
    }
}
