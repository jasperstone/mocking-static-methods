using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans;
using Orleans.Configuration;
using Orleans.Persistence.Cosmos;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.Cosmos.Tests
{
    public class CosmosStorageFactoryTests
    {
        [Fact]
        public void Create_WhenIOptionsMonitorNotRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection().BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(services, "test"));
            Assert.Contains("IOptionsMonitor<CosmosGrainStorageOptions>", exception.Message);
        }

        [Fact]
        public void Create_WithAllDependenciesRegistered_CreatesCosmosGrainStorage()
        {
            // Arrange
            var options = new CosmosGrainStorageOptions();
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get("test")).Returns(options);

            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var clusterOptions = new ClusterOptions { ServiceId = "test-service" };
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            clusterOptionsMock.Setup(o => o.Value).Returns(clusterOptions);

            var services = new ServiceCollection();
            services.AddSingleton(optionsMonitorMock.Object);
            services.AddSingleton<IPartitionKeyProvider>(partitionKeyProviderMock.Object);
            services.AddSingleton(loggerFactoryMock.Object);
            services.AddSingleton(clusterOptionsMock.Object);
            services.AddSingleton<IServiceProvider>(services.BuildServiceProvider());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CosmosGrainStorage>(result);
        }

        [Fact]
        public void Create_WhenKeyedPartitionKeyProviderNotFound_UsesNonKeyedFallback()
        {
            // Arrange
            var options = new CosmosGrainStorageOptions();
            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get("test")).Returns(options);

            var fallbackPartitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var clusterOptions = new ClusterOptions { ServiceId = "test-service" };
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            clusterOptionsMock.Setup(o => o.Value).Returns(clusterOptions);

            var services = new ServiceCollection();
            services.AddSingleton(optionsMonitorMock.Object);
            // Only non-keyed service registered (no keyed service for "test")
            services.AddSingleton<IPartitionKeyProvider>(fallbackPartitionKeyProviderMock.Object);
            services.AddSingleton(loggerFactoryMock.Object);
            services.AddSingleton(clusterOptionsMock.Object);
            services.AddSingleton<IServiceProvider>(services.BuildServiceProvider());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            Assert.NotNull(result);
        }
    }
}
