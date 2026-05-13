using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Persistence.Cosmos;

namespace Orleans.Persistence.Cosmos.Tests
{
    public class CosmosGrainStorageTests
    {
        [Fact]
        public void Create_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<CosmosGrainStorageOptions>();
            services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
            services.AddSingleton<ILoggerFactory>(Mock.Of<ILoggerFactory>());
            services.AddSingleton<IOptions<ClusterOptions>>(Mock.Of<IOptions<ClusterOptions>>());
            services.AddSingleton<IActivatorProvider>(Mock.Of<IActivatorProvider>());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var storage = CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>();
            Assert.NotNull(optionsMonitor);
        }
    }
}
