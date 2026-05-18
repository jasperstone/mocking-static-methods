using Xunit;
using Orleans.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Clustering.AzureStorage;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Runtime.MembershipService;
using Orleans.Messaging;

namespace Orleans.Clustering.AzureStorage.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_ShouldRegisterServicesCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var siloBuilderMock = new Mock<ISiloBuilder>();
            siloBuilderMock.Setup(s => s.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(services => serviceCollection = new ServiceCollection());

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(siloBuilderMock.Object, (Action<OptionsBuilder<AzureStorageClusteringOptions>>)null);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IMembershipTable>());
            Assert.NotNull(serviceProvider.GetService<IConfigurationValidator>());
        }

        [Fact]
        public void UseAzureStorageClustering_ShouldConfigureOptionsCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var siloBuilderMock = new Mock<ISiloBuilder>();
            siloBuilderMock.Setup(s => s.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(services => serviceCollection = new ServiceCollection());

            // Act
            AzureTableClusteringExtensions.UseAzureStorageClustering(siloBuilderMock.Object, options =>
            {
                options.Configure(o => o.TableName = "TestTable");
            });

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>();
            var options = optionsMonitor.Get(Options.DefaultName);
            Assert.Equal("TestTable", options.TableName);
        }
    }
}
