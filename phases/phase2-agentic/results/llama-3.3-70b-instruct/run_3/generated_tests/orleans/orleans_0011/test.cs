using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Clustering;
using Orleans.Clustering.AzureStorage;
using Orleans.Clustering.AzureStorage.Options;
using Xunit;

namespace Orleans.Clustering.AzureStorage.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_ConfiguresServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new SiloBuilder(services);

            // Act
            builder.UseAzureStorageClustering(options =>
            {
                options.StorageAccountName = "test-account";
                options.StorageAccountKey = "test-key";
            });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AzureStorageClusteringOptionsValidator>(validator);
        }

        [Fact]
        public void UseAzureStorageClustering_GetRequiredService_CallsCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new SiloBuilder(services);

            // Act
            builder.UseAzureStorageClustering(options =>
            {
                options.StorageAccountName = "test-account";
                options.StorageAccountKey = "test-key";
            });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>();
            Assert.NotNull(monitor);
        }
    }
}
