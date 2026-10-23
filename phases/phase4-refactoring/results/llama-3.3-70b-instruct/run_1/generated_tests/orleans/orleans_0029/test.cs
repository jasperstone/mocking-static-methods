using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Persistence.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;

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
            services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
            services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var storage = CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            Assert.NotNull(storage);
        }

        [Fact]
        public void Create_ThrowsException_WhenGetRequiredServiceFails()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<CosmosGrainStorageOptions>();
            services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
            services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
            services.AddSingleton<IOptions<ClusterOptions>>(Options.Create(new ClusterOptions()));
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => CosmosStorageFactory.Create(serviceProvider, null));
        }
    }
}
