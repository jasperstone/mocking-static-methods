using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Persistence.Cosmos;
using Xunit;

namespace Orleans.Persistence.Cosmos.Tests
{
    public class CosmosStorageFactoryTests
    {
        public class ClusterOptions
        {
            public string ServiceId { get; set; } = string.Empty;
        }

        public interface IActivatorProvider { }

        [Fact]
        public void Create_ShouldResolveRequiredServicesAndReturnCosmosGrainStorage()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>(MockBehavior.Strict);
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>(MockBehavior.Strict);
            var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);
            var loggerMock = new Mock<ILogger<CosmosGrainStorage>>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>(MockBehavior.Strict);
            var activatorProviderMock = new Mock<IActivatorProvider>(MockBehavior.Strict);

            var name = "TestStorage";
            var cosmosOptions = new CosmosGrainStorageOptions();

            // Setup GetService calls to simulate GetRequiredService
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)))
                .Returns(optionsMonitorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IPartitionKeyProvider)))
                .Returns(partitionKeyProviderMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
                .Returns(loggerFactoryMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptions<ClusterOptions>)))
                .Returns(clusterOptionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IActivatorProvider)))
                .Returns(activatorProviderMock.Object);

            optionsMonitorMock.Setup(m => m.Get(name)).Returns(cosmosOptions);
            clusterOptionsMock.SetupGet(c => c.Value).Returns(new ClusterOptions { ServiceId = "TestServiceId" });
            loggerFactoryMock.Setup(lf => lf.CreateLogger(typeof(CosmosGrainStorage))).Returns(loggerMock.Object);

            // Act
            var storage = CosmosStorageFactory.Create(serviceProviderMock.Object, name);

            // Assert
            Assert.NotNull(storage);
            Assert.IsType<CosmosGrainStorage>(storage);

            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<CosmosGrainStorageOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IPartitionKeyProvider)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptions<ClusterOptions>)), Times.Once);
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IActivatorProvider)), Times.Once);
            optionsMonitorMock.Verify(m => m.Get(name), Times.Once);
            clusterOptionsMock.VerifyGet(c => c.Value, Times.Once);
            loggerFactoryMock.Verify(lf => lf.CreateLogger(typeof(CosmosGrainStorage)), Times.Once);
        }
    }
}
