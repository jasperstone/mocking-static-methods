using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Orleans.Persistence.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;

namespace Orleans.Tests
{
    public class CosmosStorageFactoryTests
    {
        [Fact]
        public void Create_Should_Call_GetRequiredService_For_CosmosGrainStorageOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
            var activatorProviderMock = new Mock<IActivatorProvider>();

            var options = new CosmosGrainStorageOptions
            {
                OperationExecutor = null, // can be null or a mock
                PartitionKeyPath = "/Partition"
            };

            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Setup other required services
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>())
                .Returns(new OptionsWrapper<ClusterOptions>(new ClusterOptions { ServiceId = "TestService" }));
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IPartitionKeyProvider>())
                .Returns(partitionKeyProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IActivatorProvider>())
                .Returns(activatorProviderMock.Object);

            // Act
            var storage = CosmosStorageFactory.Create(serviceProviderMock.Object, "TestStorage");

            // Assert
            Assert.NotNull(storage);
            Assert.IsType<CosmosGrainStorage>(storage);
            optionsMonitorMock.Verify(m => m.Get("TestStorage"), Times.Once);
        }
    }
}
