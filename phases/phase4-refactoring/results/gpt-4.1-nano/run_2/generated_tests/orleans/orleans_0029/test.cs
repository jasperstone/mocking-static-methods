using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Persistence.Cosmos;
using Orleans;
using Orleans.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;

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
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger<CosmosGrainStorage>>();
            loggerFactoryMock.Setup(lf => lf.CreateLogger<CosmosGrainStorage>()).Returns(loggerMock.Object);

            // Setup the service provider to return the options monitor when requested
            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)))
                .Returns(optionsMonitorMock.Object);

            // Also setup other required services
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            clusterOptionsMock.Setup(c => c.Value).Returns(new ClusterOptions { ServiceId = "TestService" });
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
            var activatorProviderMock = new Mock<IActivatorProvider>();

            // Setup options monitor to return a default options object
            var options = new CosmosGrainStorageOptions();
            optionsMonitorMock.Setup(om => om.Get(It.IsAny<string>())).Returns(options);

            // Act
            var storage = CosmosStorageFactory.Create(
                serviceProviderMock.Object,
                "TestStorage");

            // Assert
            // Verify that GetRequiredService was called for IOptionsMonitor<CosmosGrainStorageOptions>
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)), Times.Once);
            Assert.NotNull(storage);
        }
    }
}
