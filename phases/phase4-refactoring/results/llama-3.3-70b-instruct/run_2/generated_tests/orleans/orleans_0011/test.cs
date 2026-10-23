using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Clustering.AzureStorage;
using Moq;
using Microsoft.Extensions.Options;

namespace Orleans.Hosting.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_ValidOptions_ConfiguresServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<OptionsBuilder<AzureStorageClusteringOptions>>(options =>
            {
                options.Configure(options => options.TableName = "TestTable");
            });

            // Act
            var builder = new Mock<ISiloBuilder>();
            builder.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>())).Callback((Action<IServiceCollection> callback) =>
            {
                callback(services);
            });
            builder.Object.UseAzureStorageClustering(configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var azureStorageClusteringOptions = serviceProvider.GetService<IOptions<AzureStorageClusteringOptions>>();
            Assert.NotNull(azureStorageClusteringOptions);
            Assert.Equal("TestTable", azureStorageClusteringOptions.Value.TableName);
        }

        [Fact]
        public void UseAzureStorageClustering_InvalidOptions_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<OptionsBuilder<AzureStorageClusteringOptions>>(options =>
            {
                options.Configure(options => options.TableName = null);
            });

            // Act and Assert
            var builder = new Mock<ISiloBuilder>();
            builder.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>())).Callback((Action<IServiceCollection> callback) =>
            {
                callback(services);
            });
            Assert.Throws<InvalidOperationException>(() => builder.Object.UseAzureStorageClustering(configureOptions));
        }

        [Fact]
        public void UseAzureStorageClustering_GetRequiredService_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<OptionsBuilder<AzureStorageClusteringOptions>>(options =>
            {
                options.Configure(options => options.TableName = "TestTable");
            });
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>()).Returns(new OptionsMonitor<AzureStorageClusteringOptions>(new AzureStorageClusteringOptions()));

            // Act
            var builder = new Mock<ISiloBuilder>();
            builder.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>())).Callback((Action<IServiceCollection> callback) =>
            {
                callback(services);
            });
            builder.Object.UseAzureStorageClustering(configureOptions);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>(), Times.Once);
        }
    }
}
