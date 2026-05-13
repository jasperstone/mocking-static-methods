using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.AzureUtils;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Orleans.Runtime.MembershipService;
using Xunit;

namespace Orleans.Hosting.Tests
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
                options.Configure(options => { });
            });

            // Act
            var builder = new SiloBuilder(services);
            builder.UseAzureStorageClustering(configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>();
            var options = optionsMonitor.Get(Options.DefaultName);
            Assert.NotNull(options);
        }

        [Fact]
        public void UseAzureStorageClustering_InvalidOptions_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<OptionsBuilder<AzureStorageClusteringOptions>>(options =>
            {
                options.Configure(options => { throw new Exception("Invalid options"); });
            });

            // Act and Assert
            var builder = new SiloBuilder(services);
            Assert.Throws<Exception>(() => builder.UseAzureStorageClustering(configureOptions));
        }
    }
}
