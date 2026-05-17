using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Persistence.AzureStorage;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public void Create_AzureTableGrainStorageFactory_ReturnsAzureTableGrainStorage()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<AzureTableStorageOptions>(options =>
            {
                options.TableName = "tableName";
                options.GrainStorageSerializer = new JsonGrainStorageSerializer(new OrleansJsonSerializer());
                options.DeleteStateOnClear = true;
            });
            services.AddSingleton<IOptions<ClusterOptions>>(new OptionsWrapper<ClusterOptions>(new ClusterOptions
            {
                ClusterId = "clusterId",
                ServiceId = "serviceId"
            }));
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var azureTableGrainStorage = AzureTableGrainStorageFactory.Create(serviceProvider, "name");

            // Assert
            Assert.NotNull(azureTableGrainStorage);
            Assert.IsType<AzureTableGrainStorage>(azureTableGrainStorage);
        }

        [Fact]
        public void Create_AzureTableGrainStorageFactory_ThrowsException_WhenAzureTableStorageOptionsIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddSingleton<IOptions<ClusterOptions>>(new OptionsWrapper<ClusterOptions>(new ClusterOptions
            {
                ClusterId = "clusterId",
                ServiceId = "serviceId"
            }));
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => AzureTableGrainStorageFactory.Create(serviceProvider, "name"));
        }

        [Fact]
        public void Create_AzureTableGrainStorageFactory_ThrowsException_WhenClusterOptionsIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions();
            services.Configure<AzureTableStorageOptions>(options =>
            {
                options.TableName = "tableName";
                options.GrainStorageSerializer = new JsonGrainStorageSerializer(new OrleansJsonSerializer());
                options.DeleteStateOnClear = true;
            });
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => AzureTableGrainStorageFactory.Create(serviceProvider, "name"));
        }
    }
}
