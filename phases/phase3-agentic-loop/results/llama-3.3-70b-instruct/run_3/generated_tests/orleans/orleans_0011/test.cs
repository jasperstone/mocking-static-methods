using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Orleans.AzureUtils;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Moq;
using Microsoft.Extensions.Options;

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
            var builder = new Mock<ISiloBuilder>();
            builder.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>())).Callback<Action<IServiceCollection>>(action =>
            {
                action(services);
            });
            AzureTableClusteringExtensions.UseAzureStorageClustering(builder.Object, configureOptions);

            // Assert
            Assert.Contains(services, s => s.ServiceType == typeof(IMembershipTable));
            Assert.Contains(services, s => s.ServiceType == typeof(IConfigurationValidator));
        }

        [Fact]
        public void UseAzureStorageClustering_GetRequiredService_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var configureOptions = new Action<OptionsBuilder<AzureStorageClusteringOptions>>(options =>
            {
                options.Configure(options => { });
            });
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IOptionsMonitor<AzureStorageClusteringOptions>))).Returns(new Mock<IOptionsMonitor<AzureStorageClusteringOptions>>().Object);

            // Act
            var builder = new Mock<ISiloBuilder>();
            builder.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>())).Callback<Action<IServiceCollection>>(action =>
            {
                action(services);
            });
            AzureTableClusteringExtensions.UseAzureStorageClustering(builder.Object, configureOptions);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(IOptionsMonitor<AzureStorageClusteringOptions>)), Times.Once);
        }
    }
}
