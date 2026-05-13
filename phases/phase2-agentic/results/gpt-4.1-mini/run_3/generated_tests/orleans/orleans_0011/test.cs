using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime.MembershipService;
using Xunit;

namespace Orleans.Clustering.AzureStorage.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_WithConfigureOptions_RegistersServicesAndValidator()
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
                // Configure some option to test the configureOptions delegate is called
                optionsBuilder.Configure(o => o.TableName = "TestTable");
            });

            // Assert
            Assert.Same(builder, returnedBuilder);

            var serviceProvider = services.BuildServiceProvider();

            // Validate that AzureStorageClusteringOptions is configured with the delegate
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>();
            var options = optionsMonitor.Get(Options.DefaultName);
            Assert.Equal("TestTable", options.TableName);

            // Validate that IMembershipTable is registered as AzureBasedMembershipTable singleton
            var membershipTable = serviceProvider.GetService<IMembershipTable>();
            Assert.NotNull(membershipTable);
            Assert.IsType<AzureBasedMembershipTable>(membershipTable);

            // Validate that IConfigurationValidator is registered and can be resolved
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AzureStorageClusteringOptionsValidator>(validator);
        }

        [Fact]
        public void UseAzureStorageClustering_WithNullConfigureOptions_RegistersServicesAndValidator()
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

            // Validate that IMembershipTable is registered as AzureBasedMembershipTable singleton
            var membershipTable = serviceProvider.GetService<IMembershipTable>();
            Assert.NotNull(membershipTable);
            Assert.IsType<AzureBasedMembershipTable>(membershipTable);

            // Validate that IConfigurationValidator is registered and can be resolved
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AzureStorageClusteringOptionsValidator>(validator);
        }
    }
}
