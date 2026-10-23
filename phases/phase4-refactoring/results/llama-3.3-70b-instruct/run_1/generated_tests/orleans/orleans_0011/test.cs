using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.AzureUtils;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Orleans.Messaging;
using Orleans.Runtime.MembershipService;
using Moq;

namespace Orleans.Hosting.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_ConfiguresServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<OptionsBuilder<AzureStorageClusteringOptions>>(options =>
            {
                options.Configure(options => { });
            });

            // Act
            services.AddOptions<AzureStorageClusteringOptions>();
            configureOptions(new OptionsBuilder<AzureStorageClusteringOptions>(services, "AzureStorageClusteringOptions"));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var membershipTable = serviceProvider.GetService<IMembershipTable>();
            Assert.NotNull(membershipTable);
        }

        [Fact]
        public void UseAzureStorageClustering_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<OptionsBuilder<AzureStorageClusteringOptions>>(options =>
            {
                options.Configure(options => { });
            });
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>())
                .Returns(new Mock<IOptionsMonitor<AzureStorageClusteringOptions>>().Object);

            // Act
            services.AddOptions<AzureStorageClusteringOptions>();
            configureOptions(new OptionsBuilder<AzureStorageClusteringOptions>(services, "AzureStorageClusteringOptions"));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var configurationValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);
        }
    }
}
