using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Messaging;
using Orleans.Runtime.MembershipService;
using Xunit;

namespace Orleans.Clustering.AzureStorage.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_WithConfigureOptions_Action_ConfiguresServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(configure =>
                {
                    configure(services);
                })
                .Returns(builderMock.Object);

            var builder = builderMock.Object;

            // Act
            var returnedBuilder = builder.UseAzureStorageClustering(optionsBuilder =>
            {
                // Configure some option to test the invocation
                optionsBuilder.Configure(o => o.TableName = "TestTable");
            });

            // Assert
            Assert.Same(builder, returnedBuilder);

            var serviceProvider = services.BuildServiceProvider();

            // Check that IOptionsMonitor<AzureStorageClusteringOptions> is registered
            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<AzureStorageClusteringOptions>>();
            Assert.NotNull(optionsMonitor);

            // Check that IConfigurationValidator is registered and is AzureStorageClusteringOptionsValidator
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AzureStorageClusteringOptionsValidator>(validator);

            // Check that IMembershipTable is registered and is AzureBasedMembershipTable
            var membershipTable = serviceProvider.GetService<IMembershipTable>();
            Assert.NotNull(membershipTable);
            Assert.IsType<AzureBasedMembershipTable>(membershipTable);
        }

        [Fact]
        public void UseAzureStorageClustering_WithNullConfigureOptions_Action_ConfiguresServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(configure =>
                {
                    configure(services);
                })
                .Returns(builderMock.Object);

            var builder = builderMock.Object;

            // Act
            var returnedBuilder = builder.UseAzureStorageClustering((Action<OptionsBuilder<AzureStorageClusteringOptions>>)null);

            // Assert
            Assert.Same(builder, returnedBuilder);

            var serviceProvider = services.BuildServiceProvider();

            // Check that IConfigurationValidator is registered and is AzureStorageClusteringOptionsValidator
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AzureStorageClusteringOptionsValidator>(validator);

            // Check that IMembershipTable is registered and is AzureBasedMembershipTable
            var membershipTable = serviceProvider.GetService<IMembershipTable>();
            Assert.NotNull(membershipTable);
            Assert.IsType<AzureBasedMembershipTable>(membershipTable);
        }
    }
}
