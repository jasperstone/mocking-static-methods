using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Orleans.Persistence.Cosmos;

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
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<CosmosGrainStorage>>();
            loggerFactoryMock
                .Setup(lf => lf.CreateLogger<CosmosGrainStorage>())
                .Returns(loggerMock.Object);

            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            clusterOptionsMock.Setup(co => co.Value).Returns(new ClusterOptions { ServiceId = "test-service" });

            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
            var activatorProviderMock = new Mock<IActivatorProvider>();

            // Setup the options monitor to return a dummy options object
            var dummyOptions = new CosmosGrainStorageOptions();
            optionsMonitorMock
                .Setup(om => om.Get(It.IsAny<string>()))
                .Returns(dummyOptions);

            // Act
            var storage = CosmosStorageFactory.Create(
                serviceProviderMock.Object,
                "TestProvider"
            );

            // Assert
            // Verify that GetRequiredService was called for IOptionsMonitor<CosmosGrainStorageOptions>
            serviceProviderMock.Verify(sp => sp.GetRequiredService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)), Times.Once);
        }
    }
}
