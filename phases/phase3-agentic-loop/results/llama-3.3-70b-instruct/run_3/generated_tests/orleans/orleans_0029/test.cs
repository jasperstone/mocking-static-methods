using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Persistence.Cosmos;
using Microsoft.Extensions.Options;
using Orleans;

namespace Orleans.Persistence.Cosmos.Tests
{
    public class CosmosStorageFactoryTests
    {
        [Fact]
        public void Create_CallsGetRequiredService_ForOptionsMonitor()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(Mock.Of<IOptionsMonitor<CosmosGrainStorageOptions>>());
            services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());
            services.AddSingleton<IOptions<Orleans.Configuration.ClusterOptions>>(Mock.Of<IOptions<Orleans.Configuration.ClusterOptions>>());
            services.AddSingleton<Orleans.Runtime.IActivatorProvider>(Mock.Of<Orleans.Runtime.IActivatorProvider>());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var storage = CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<CosmosGrainStorageOptions>>();
            Assert.NotNull(optionsMonitor);
        }

        [Fact]
        public void Create_CallsGetRequiredService_ForPartitionKeyProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(Mock.Of<IOptionsMonitor<CosmosGrainStorageOptions>>());
            services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());
            services.AddSingleton<IOptions<Orleans.Configuration.ClusterOptions>>(Mock.Of<IOptions<Orleans.Configuration.ClusterOptions>>());
            services.AddSingleton<Orleans.Runtime.IActivatorProvider>(Mock.Of<Orleans.Runtime.IActivatorProvider>());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var storage = CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            var partitionKeyProvider = serviceProvider.GetRequiredService<IPartitionKeyProvider>();
            Assert.NotNull(partitionKeyProvider);
        }

        [Fact]
        public void Create_CallsGetRequiredService_ForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(Mock.Of<IOptionsMonitor<CosmosGrainStorageOptions>>());
            services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());
            services.AddSingleton<IOptions<Orleans.Configuration.ClusterOptions>>(Mock.Of<IOptions<Orleans.Configuration.ClusterOptions>>());
            services.AddSingleton<Orleans.Runtime.IActivatorProvider>(Mock.Of<Orleans.Runtime.IActivatorProvider>());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var storage = CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            var loggerFactory = serviceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
            Assert.NotNull(loggerFactory);
        }

        [Fact]
        public void Create_CallsGetRequiredService_ForClusterOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(Mock.Of<IOptionsMonitor<CosmosGrainStorageOptions>>());
            services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());
            services.AddSingleton<IOptions<Orleans.Configuration.ClusterOptions>>(Mock.Of<IOptions<Orleans.Configuration.ClusterOptions>>());
            services.AddSingleton<Orleans.Runtime.IActivatorProvider>(Mock.Of<Orleans.Runtime.IActivatorProvider>());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var storage = CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            var clusterOptions = serviceProvider.GetRequiredService<IOptions<Orleans.Configuration.ClusterOptions>>();
            Assert.NotNull(clusterOptions);
        }

        [Fact]
        public void Create_CallsGetRequiredService_ForActivatorProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(Mock.Of<IOptionsMonitor<CosmosGrainStorageOptions>>());
            services.AddSingleton<IPartitionKeyProvider>(Mock.Of<IPartitionKeyProvider>());
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(Mock.Of<Microsoft.Extensions.Logging.ILoggerFactory>());
            services.AddSingleton<IOptions<Orleans.Configuration.ClusterOptions>>(Mock.Of<IOptions<Orleans.Configuration.ClusterOptions>>());
            services.AddSingleton<Orleans.Runtime.IActivatorProvider>(Mock.Of<Orleans.Runtime.IActivatorProvider>());
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var storage = CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            var activatorProvider = serviceProvider.GetRequiredService<Orleans.Runtime.IActivatorProvider>();
            Assert.NotNull(activatorProvider);
        }
    }
}
