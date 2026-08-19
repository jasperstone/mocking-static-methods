using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Providers.Azure;
using Orleans.Runtime;
using Orleans.Serialization.Serializers;
using Xunit;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;

namespace Orleans.Storage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void CreateAzureTableGrainStorage_WithValidOptions_ReturnsInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<AzureTableStorageOptions>()
                .Configure(options =>
                {
                    options.TableName = "TestTable";
                    options.GrainStorageSerializer = new DefaultGrainStorageSerializer();
                });
            services.AddSingleton<ILogger<AzureTableGrainStorage>>(_ => new LoggerFactory().CreateLogger<AzureTableGrainStorage>());
            services.AddSingleton<IActivatorProvider>(_ => new DefaultActivatorProvider());
            services.AddSingleton<IOptions<ClusterOptions>>(_ => Options.Create(new ClusterOptions()));
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var instance = AzureTableGrainStorageFactory.Create(serviceProvider, "TestProvider");

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CreateAzureTableGrainStorage_WithInvalidOptions_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<AzureTableStorageOptions>()
                .Configure(options =>
                {
                    options.TableName = null;
                    options.GrainStorageSerializer = new DefaultGrainStorageSerializer();
                });
            services.AddSingleton<ILogger<AzureTableGrainStorage>>(_ => new LoggerFactory().CreateLogger<AzureTableGrainStorage>());
            services.AddSingleton<IActivatorProvider>(_ => new DefaultActivatorProvider());
            services.AddSingleton<IOptions<ClusterOptions>>(_ => Options.Create(new ClusterOptions()));
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => AzureTableGrainStorageFactory.Create(serviceProvider, "TestProvider"));
        }
    }
}
