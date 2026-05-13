using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Persistence.Cosmos;

namespace Orleans.Persistence.Cosmos.Tests
{
    public class CosmosStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldCallGetRequiredServiceForIOptionsMonitorCosmosGrainStorageOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            var activatorProviderMock = new Mock<IActivatorProvider>();

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            serviceProviderMock
                .Setup(s => s.GetKeyedService<IPartitionKeyProvider>("test"))
                .Returns((IPartitionKeyProvider)null);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IPartitionKeyProvider>())
                .Returns(partitionKeyProviderMock.Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IOptions<ClusterOptions>>())
                .Returns(clusterOptionsMock.Object);

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IActivatorProvider>())
                .Returns(activatorProviderMock.Object);

            // Act
            var result = CosmosStorageFactory.Create(serviceProviderMock.Object, "test");

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>(), Times.Once);
            Assert.NotNull(result);
        }
    }
}
