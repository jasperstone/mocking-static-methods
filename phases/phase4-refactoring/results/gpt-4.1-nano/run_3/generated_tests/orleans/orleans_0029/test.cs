using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Persistence.Cosmos;
using Orleans;
using Orleans.Storage;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Orleans.Tests
{
    public class CosmosStorageFactoryTests
    {
        [Fact]
        public void Create_Should_Call_GetRequiredService_ForOptionsMonitor()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();

            // Setup the service provider to return the options monitor when requested
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Also setup other required services
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
            var activatorProviderMock = new Mock<IActivatorProvider>();

            // Setup the cluster options to return a dummy service id
            var clusterOptions = new ClusterOptions { ServiceId = "test-service" };
            clusterOptionsMock.Setup(c => c.Value).Returns(clusterOptions);

            // Setup the service provider to return other required services
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(IOptions<ClusterOptions>)))
                .Returns(clusterOptionsMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(IPartitionKeyProvider)))
                .Returns(partitionKeyProviderMock.Object);
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(IActivatorProvider)))
                .Returns(activatorProviderMock.Object);

            // Setup options monitor to return a dummy options object
            var options = new CosmosGrainStorageOptions();
            optionsMonitorMock.Setup(om => om.Get(It.IsAny<string>())).Returns(options);

            // Act
            var storage = CosmosStorageFactory.Create(serviceProviderMock.Object, "test-grain");

            // Assert
            // Verify that GetRequiredService was called for IOptionsMonitor<CosmosGrainStorageOptions>
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IOptions<CosmosGrainStorageOptions>)), Times.Once);
            Assert.NotNull(storage);
        }
    }
}
