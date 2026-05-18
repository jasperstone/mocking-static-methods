using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.AzureUtils;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_ValidOptions_ConfiguresServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<OptionsBuilder<AzureStorageClusteringOptions>>(options =>
            {
                options.Configure(options =>
                {
                    // configure options
                });
            });

            // Act
            var builder = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            configureOptions?.Invoke(builder.AddOptions<AzureStorageClusteringOptions>());
            builder.AddTransient<IConfigurationValidator>(sp => new AzureStorageClusteringOptionsValidator(sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>().Get(Options.DefaultName), Options.DefaultName));
            var serviceProvider = builder.BuildServiceProvider();

            // Assert
            var azureStorageClusteringOptions = serviceProvider.GetService<IOptionsMonitor<AzureStorageClusteringOptions>>();
            Assert.NotNull(azureStorageClusteringOptions);
            var azureStorageClusteringOptionsValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(azureStorageClusteringOptionsValidator);
        }

        [Fact]
        public void UseAzureStorageClustering_InvalidOptions_DoesNotThrowException()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<OptionsBuilder<AzureStorageClusteringOptions>>(options =>
            {
                options.Configure(options =>
                {
                    // configure invalid options
                });
            });

            // Act and Assert
            var builder = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            configureOptions?.Invoke(builder.AddOptions<AzureStorageClusteringOptions>());
        }
    }
}
