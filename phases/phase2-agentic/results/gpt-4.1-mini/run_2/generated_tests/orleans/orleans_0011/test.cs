using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Clustering.AzureStorage;
using Orleans.Configuration;
using Orleans.Hosting;
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
                .Callback<Action<IServiceCollection>>(configureAction =>
                {
                    configureAction(services);
                })
                .Returns(builderMock.Object);

            var builder = builderMock.Object;

            // Act
            var returnedBuilder = builder.UseAzureStorageClustering(optionsBuilder =>
            {
                optionsBuilder.Configure(options =>
                {
                    options.ConnectionString = "UseDevelopmentStorage=true";
                });
            });

            // Assert
            Assert.Same(builder, returnedBuilder);

            var serviceProvider = services.BuildServiceProvider();

            // Check that IMembershipTable is registered as singleton and is AzureBasedMembershipTable
            var membershipTable = serviceProvider.GetService<IMembershipTable>();
            Assert.NotNull(membershipTable);
            Assert.IsType<AzureBasedMembershipTable>(membershipTable);

            // Check that IConfigurationValidator is registered as transient and resolves correctly
            var validator1 = serviceProvider.GetService<IConfigurationValidator>();
            var validator2 = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator1);
            Assert.NotNull(validator2);
            Assert.NotSame(validator1, validator2);

            // The validator should be AzureStorageClusteringOptionsValidator
            Assert.IsType<AzureStorageClusteringOptionsValidator>(validator1);

            // Check that IOptionsMonitor<AzureStorageClusteringOptions> is registered
            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<AzureStorageClusteringOptions>>();
            Assert.NotNull(optionsMonitor);

            // Check that the validator was constructed with the default options name
            var validator = (AzureStorageClusteringOptionsValidator)validator1;
            Assert.Equal(Options.DefaultName, validator.OptionsName);
        }

        [Fact]
        public void UseAzureStorageClustering_WithNullConfigureOptions_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderMock = new Mock<ISiloBuilder>();
            builderMock.Setup(b => b.ConfigureServices(It.IsAny<Action<IServiceCollection>>()))
                .Callback<Action<IServiceCollection>>(configureAction =>
                {
                    configureAction(services);
                })
                .Returns(builderMock.Object);

            var builder = builderMock.Object;

            // Act & Assert
            var ex = Record.Exception(() => builder.UseAzureStorageClustering((Action<OptionsBuilder<AzureStorageClusteringOptions>>)null));
            Assert.Null(ex);

            var serviceProvider = services.BuildServiceProvider();

            // Check that IMembershipTable is registered as singleton and is AzureBasedMembershipTable
            var membershipTable = serviceProvider.GetService<IMembershipTable>();
            Assert.NotNull(membershipTable);
            Assert.IsType<AzureBasedMembershipTable>(membershipTable);
        }
    }
}
