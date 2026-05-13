using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.Persistence.Cosmos;
using System;
using System.Threading.Tasks;
using System.Net;

namespace Orleans.Tests
{
    public class CosmosStorageFactoryTests
    {
        [Fact]
        public void Create_ShouldResolveRequiredServices()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<CosmosGrainStorageOptions>>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
            var activatorProviderMock = new Mock<IActivatorProvider>();
            var storageOptions = new CosmosGrainStorageOptions();

            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(storageOptions);

            var clusterOptions = new ClusterOptions { ServiceId = "test-service" };
            var clusterOptionsWrapper = Options.Create(clusterOptions);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(Options.Create(clusterOptions));
            serviceCollection.AddSingleton(ILoggerFactory, loggerFactoryMock.Object);
            serviceCollection.AddSingleton<IPartitionKeyProvider>(partitionKeyProviderMock.Object);
            serviceCollection.AddSingleton<IActivatorProvider>(activatorProviderMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            servicesMock.Setup(s => s.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<IPartitionKeyProvider>())
                .Returns(partitionKeyProviderMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<IActivatorProvider>())
                .Returns(activatorProviderMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<IOptions<ClusterOptions>>())
                .Returns(clusterOptionsWrapper);

            // Act
            var storage = CosmosStorageFactory.Create(servicesMock.Object, "TestStorage");

            // Assert
            Assert.NotNull(storage);
        }

        [Fact]
        public async Task Create_ShouldCallGetRequiredService_ForOptionsMonitor()
        {
            // Arrange
            var servicesMock = new Mock<IServiceProvider>();
            var optionsMock = new Mock<IOptions<CosmosGrainStorageOptions>>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var partitionKeyProviderMock = new Mock<IPartitionKeyProvider>();
            var activatorProviderMock = new Mock<IActivatorProvider>();
            var storageOptions = new CosmosGrainStorageOptions();

            var optionsMonitorMock = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(storageOptions);

            var clusterOptions = new ClusterOptions { ServiceId = "test-service" };
            var clusterOptionsWrapper = Options.Create(clusterOptions);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(Options.Create(clusterOptions));
            serviceCollection.AddSingleton(ILoggerFactory, loggerFactoryMock.Object);
            serviceCollection.AddSingleton<IPartitionKeyProvider>(partitionKeyProviderMock.Object);
            serviceCollection.AddSingleton<IActivatorProvider>(activatorProviderMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            servicesMock.Setup(s => s.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>())
                .Returns(optionsMonitorMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<ILoggerFactory>())
                .Returns(loggerFactoryMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<IPartitionKeyProvider>())
                .Returns(partitionKeyProviderMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<IActivatorProvider>())
                .Returns(activatorProviderMock.Object);
            servicesMock.Setup(s => s.GetRequiredService<IOptions<ClusterOptions>>())
                .Returns(clusterOptionsWrapper);

            // Act
            var storage = CosmosStorageFactory.Create(servicesMock.Object, "TestStorage");

            // Assert
            Assert.NotNull(storage);
            // Verify that GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>() was called
            optionsMonitorMock.Verify(m => m.Get(It.IsAny<string>()), Times.Once);
        }
    }
}
