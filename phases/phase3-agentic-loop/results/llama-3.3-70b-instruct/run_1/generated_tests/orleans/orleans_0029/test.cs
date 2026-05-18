using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Persistence.Cosmos;
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
            services.AddOptions<CosmosGrainStorageOptions>();
            services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(new OptionsMonitor<CosmosGrainStorageOptions>(new CosmosGrainStorageOptions()));
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(new Microsoft.Extensions.Logging.LoggerFactory());
            services.AddSingleton<IOptions<ClusterOptions>>(new Options<ClusterOptions>(new ClusterOptions()));
            services.AddSingleton<IActivatorProvider>(new ActivatorProvider());
            var serviceProvider = services.BuildServiceProvider();

            var mockOptionsMonitor = new Mock<IOptionsMonitor<CosmosGrainStorageOptions>>();
            services.AddSingleton(mockOptionsMonitor.Object);

            // Act
            CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            mockOptionsMonitor.Verify(m => m.Get("test"), Times.Once);
        }

        [Fact]
        public void Create_CallsGetRequiredService_ForPartitionKeyProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<CosmosGrainStorageOptions>();
            services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(new OptionsMonitor<CosmosGrainStorageOptions>(new CosmosGrainStorageOptions()));
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(new Microsoft.Extensions.Logging.LoggerFactory());
            services.AddSingleton<IOptions<ClusterOptions>>(new Options<ClusterOptions>(new ClusterOptions()));
            services.AddSingleton<IActivatorProvider>(new ActivatorProvider());
            var serviceProvider = services.BuildServiceProvider();

            var mockPartitionKeyProvider = new Mock<IPartitionKeyProvider>();
            services.AddSingleton(mockPartitionKeyProvider.Object);

            // Act
            CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            mockPartitionKeyProvider.Verify(m => m.GetPartitionKey(It.IsAny<string>(), It.IsAny<GrainId>()), Times.Once);
        }

        [Fact]
        public void Create_CallsGetRequiredService_ForLoggerFactory()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<CosmosGrainStorageOptions>();
            services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(new OptionsMonitor<CosmosGrainStorageOptions>(new CosmosGrainStorageOptions()));
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(new Microsoft.Extensions.Logging.LoggerFactory());
            services.AddSingleton<IOptions<ClusterOptions>>(new Options<ClusterOptions>(new ClusterOptions()));
            services.AddSingleton<IActivatorProvider>(new ActivatorProvider());
            var serviceProvider = services.BuildServiceProvider();

            var mockLoggerFactory = new Mock<Microsoft.Extensions.Logging.ILoggerFactory>();
            services.AddSingleton(mockLoggerFactory.Object);

            // Act
            CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            mockLoggerFactory.Verify(m => m.CreateLogger(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Create_CallsGetRequiredService_ForClusterOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<CosmosGrainStorageOptions>();
            services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(new OptionsMonitor<CosmosGrainStorageOptions>(new CosmosGrainStorageOptions()));
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(new Microsoft.Extensions.Logging.LoggerFactory());
            services.AddSingleton<IOptions<ClusterOptions>>(new Options<ClusterOptions>(new ClusterOptions()));
            services.AddSingleton<IActivatorProvider>(new ActivatorProvider());
            var serviceProvider = services.BuildServiceProvider();

            var mockClusterOptions = new Mock<IOptions<ClusterOptions>>();
            services.AddSingleton(mockClusterOptions.Object);

            // Act
            CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            mockClusterOptions.Verify(m => m.Value, Times.Once);
        }

        [Fact]
        public void Create_CallsGetRequiredService_ForActivatorProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<CosmosGrainStorageOptions>();
            services.AddSingleton<IOptionsMonitor<CosmosGrainStorageOptions>>(new OptionsMonitor<CosmosGrainStorageOptions>(new CosmosGrainStorageOptions()));
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(new Microsoft.Extensions.Logging.LoggerFactory());
            services.AddSingleton<IOptions<ClusterOptions>>(new Options<ClusterOptions>(new ClusterOptions()));
            services.AddSingleton<IActivatorProvider>(new ActivatorProvider());
            var serviceProvider = services.BuildServiceProvider();

            var mockActivatorProvider = new Mock<IActivatorProvider>();
            services.AddSingleton(mockActivatorProvider.Object);

            // Act
            CosmosStorageFactory.Create(serviceProvider, "test");

            // Assert
            mockActivatorProvider.Verify(m => m.GetActivator(It.IsAny<string>()), Times.Once);
        }
    }
}
