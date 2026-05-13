using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Persistence.Cosmos;
using Orleans.Storage;
using Orleans.Runtime;
using Orleans.Hosting;
using System;

namespace Orleans.Persistence.Cosmos.Tests
{
    public class CosmosStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldReturnCosmosGrainStorage()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            var activatorProviderMock = new Mock<IActivatorProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<IPartitionKeyProvider>("name"))
                .Returns(partitionKeyProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPartitionKeyProvider>())
                .Returns(partitionKeyProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>())
                .Returns(clusterOptionsMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IActivatorProvider>())
                .Returns(activatorProviderMock.Object);

            // Act
            var result = CosmosStorageFactory.Create(serviceProviderMock.Object, "name");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CosmosGrainStorage>(result);
        }

        [Fact]
        public void Create_ShouldThrowException_WhenOptionsMonitorNotRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProviderMock.Object, "name"));
        }

        [Fact]
        public void Create_ShouldThrowException_WhenPartitionKeyProviderNotRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProviderMock.Object, "name"));
        }

        [Fact]
        public void Create_ShouldThrowException_WhenLoggerFactoryNotRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<IPartitionKeyProvider>("name"))
                .Returns(partitionKeyProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPartitionKeyProvider>())
                .Returns(partitionKeyProviderMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProviderMock.Object, "name"));
        }

        [Fact]
        public void Create_ShouldThrowException_WhenClusterOptionsNotRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<IPartitionKeyProvider>("name"))
                .Returns(partitionKeyProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPartitionKeyProvider>())
                .Returns(partitionKeyProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProviderMock.Object, "name"));
        }

        [Fact]
        public void Create_ShouldThrowException_WhenActivatorProviderNotRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetKeyedService<IPartitionKeyProvider>("name"))
                .Returns(partitionKeyProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IPartitionKeyProvider>())
                .Returns(partitionKeyProviderMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>())
                .Returns(clusterOptionsMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProviderMock.Object, "name"));
        }
    }
}
