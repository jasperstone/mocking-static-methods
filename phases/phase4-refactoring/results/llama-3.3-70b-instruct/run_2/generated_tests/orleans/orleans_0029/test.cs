using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Persistence.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;

namespace Orleans.Persistence.Cosmos.Tests
{
    public class CosmosStorageFactoryTests
    {
        [Fact]
        public void Create_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<CosmosGrainStorageOptions>();
            services.AddSingleton<ILoggerFactory>();
            services.AddSingleton<IOptions<ClusterOptions>>();
            services.AddSingleton<IActivatorProvider>();
            services.AddSingleton<IPartitionKeyProvider>();
            var serviceProvider = services.BuildServiceProvider();

            var mockOptionsMonitor = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
            var mockPartitionKeyProvider = new Mock<IPartitionKeyProvider>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var mockClusterOptions = new Mock<IOptions<ClusterOptions>>();
            var mockActivatorProvider = new Mock<IActivatorProvider>();

            serviceProvider.GetService<IOptionsMonitor<CosmosGrainStorageOptions>>().Returns(mockOptionsMonitor.Object);
            serviceProvider.GetService<IPartitionKeyProvider>().Returns(mockPartitionKeyProvider.Object);
            serviceProvider.GetService<ILoggerFactory>().Returns(mockLoggerFactory.Object);
            serviceProvider.GetService<IOptions<ClusterOptions>>().Returns(mockClusterOptions.Object);
            serviceProvider.GetService<IActivatorProvider>().Returns(mockActivatorProvider.Object);

            // Act
            var factory = new CosmosStorageFactory();
            var storage = factory.Create(serviceProvider, "test");

            // Assert
            mockOptionsMonitor.Verify(m => m.Get("test"), Times.Once);
            mockPartitionKeyProvider.Verify(m => m.GetPartitionKey(It.IsAny<string>(), It.IsAny<GrainId>()), Times.Once);
            mockLoggerFactory.Verify(m => m.CreateLogger(It.IsAny<string>()), Times.Once);
            mockClusterOptions.Verify(m => m.Value, Times.Once);
            mockActivatorProvider.Verify(m => m.GetActivator(It.IsAny<string>()), Times.Once);
        }
    }
}
