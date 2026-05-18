using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Clustering.AzureStorage;
using Moq;
using System;

namespace Orleans.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_WithConfigureOptions_ShouldConfigureServicesAndResolveValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new Mock<ISiloBuilder>();
            builder.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(configure => configure(services));

            // Act
            builder.Object.UseAzureStorageClustering(opts => { opts.ConnectionString = "UseDevelopmentStorage=true"; });

            // Assert
            var serviceProvider = services.BuildServiceProvider();

            // Check that IMembershipTable is registered
            var membershipTable = serviceProvider.GetService<IMembershipTable>();
            Assert.NotNull(membershipTable);
            Assert.IsType<AzureBasedMembershipTable>(membershipTable);

            // Check that IConfigurationValidator is registered and can be resolved
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AzureStorageClusteringOptionsValidator>(validator);
        }

        [Fact]
        public void UseAzureStorageClustering_WithNullConfigureOptions_ShouldNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new Mock<ISiloBuilder>();
            builder.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(configure => configure(services));

            // Act & Assert
            var exception = Record.Exception(() => builder.Object.UseAzureStorageClustering((Action<AzureStorageClusteringOptions>)null));
            Assert.Null(exception);
        }
    }
}
