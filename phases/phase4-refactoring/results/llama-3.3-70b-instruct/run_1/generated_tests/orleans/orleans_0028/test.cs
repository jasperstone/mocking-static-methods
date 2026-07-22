using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Tests
{
    public class AzureTableGrainStorageTests
    {
        [Fact]
        public async Task CreateAzureTableGrainStorage_WithValidServiceProvider_ReturnsInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddOptions<AzureTableStorageOptions>();
            services.AddSingleton<IGrainStorageSerializer, DefaultGrainStorageSerializer>();
            services.AddSingleton<IActivatorProvider, DefaultActivatorProvider>();
            services.AddSingleton<ILogger<AzureTableGrainStorage>, Logger<AzureTableGrainStorage>>();
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var azureTableGrainStorage = AzureTableGrainStorageFactory.Create(serviceProvider, "test");

            // Assert
            Assert.NotNull(azureTableGrainStorage);
        }

        [Fact]
        public async Task CreateAzureTableGrainStorage_WithInvalidServiceProvider_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<ArgumentException>(() => AzureTableGrainStorageFactory.Create(serviceProvider, "test"));
        }
    }
}
