using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Providers.Azure;
using Orleans.Runtime;
using Orleans.Serialization;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Storage.Tests
{
    public class AzureTableGrainStorageFactoryTests
    {
        [Fact]
        public async Task Create_ValidServiceProviderAndName_ReturnsAzureTableGrainStorage()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<AzureTableStorageOptions>()
                .AddSingleton<ILogger<AzureTableGrainStorage>>(new LoggerFactory().CreateLogger<AzureTableGrainStorage>())
                .AddSingleton<IActivatorProvider, ActivatorProvider>()
                .BuildServiceProvider();

            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<AzureTableStorageOptions>>();
            optionsMonitor.CurrentValue = new AzureTableStorageOptions
            {
                StorageAccountName = "test",
                StorageAccountKey = "test",
                TableName = "test",
                GrainStorageSerializer = new JsonGrainStorageSerializer(new OrleansJsonSerializer())
            };

            var name = "test";

            // Act
            var azureTableGrainStorage = AzureTableGrainStorageFactory.Create(serviceProvider, name);

            // Assert
            Assert.NotNull(azureTableGrainStorage);
        }

        [Fact]
        public async Task Create_InvalidServiceProvider_ThrowsException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var name = "test";

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => AzureTableGrainStorageFactory.Create(serviceProvider, name));
        }
    }
}
