using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.AzureUtils;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Orleans.Hosting;
using Moq;

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
            var builder = new SiloBuilder();
            builder.UseAzureStorageClustering(configureOptions);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var azureStorageClusteringOptionsValidator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(azureStorageClusteringOptionsValidator);
            Assert.IsType<AzureStorageClusteringOptionsValidator>(azureStorageClusteringOptionsValidator);
        }

        [Fact]
        public void UseAzureStorageClustering_InvalidOptions_ThrowsException()
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
            var builder = new SiloBuilder();
            Assert.Throws<Exception>(() => builder.UseAzureStorageClustering(configureOptions));
        }

        [Fact]
        public void UseAzureStorageClustering_GetRequiredService_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureStorageClusteringOptions>>();
            var azureStorageClusteringOptions = new AzureStorageClusteringOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(azureStorageClusteringOptions);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>()).Returns(optionsMonitorMock.Object);

            // Act
            var azureStorageClusteringOptionsValidator = new AzureStorageClusteringOptionsValidator(azureStorageClusteringOptions, "Default");

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>(), Times.Once);
        }
    }
}
