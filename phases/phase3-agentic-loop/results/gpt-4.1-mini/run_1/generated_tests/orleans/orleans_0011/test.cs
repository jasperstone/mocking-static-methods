using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime.MembershipService;
using Xunit;

namespace Orleans.Clustering.AzureStorage.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        private class TestSiloBuilder : ISiloBuilder
        {
            public IServiceCollection Services { get; } = new ServiceCollection();

            public ISiloBuilder ConfigureServices(Action<IServiceCollection> configureDelegate)
            {
                configureDelegate(Services);
                return this;
            }
        }

        [Fact]
        public void UseAzureStorageClustering_WithOptionsBuilder_RegistersConfigurationValidator()
        {
            // Arrange
            var builder = new TestSiloBuilder();

            // Act
            var returnedBuilder = AzureTableClusteringExtensions.UseAzureStorageClustering(
                builder,
                (Action<OptionsBuilder<AzureStorageClusteringOptions>>)(optionsBuilder =>
                {
                    // No-op configure options
                }));

            // Assert
            Assert.Same(builder, returnedBuilder);

            var serviceProvider = builder.Services.BuildServiceProvider();

            // The IConfigurationValidator should be registered as transient
            var validator1 = serviceProvider.GetService<IConfigurationValidator>();
            var validator2 = serviceProvider.GetService<IConfigurationValidator>();

            Assert.NotNull(validator1);
            Assert.NotNull(validator2);
            Assert.NotSame(validator1, validator2);

            // The validator should be of type AzureStorageClusteringOptionsValidator
            Assert.Equal("AzureStorageClusteringOptionsValidator", validator1.GetType().Name);

            // The IMembershipTable should be registered as singleton
            var membershipTable1 = serviceProvider.GetService<IMembershipTable>();
            var membershipTable2 = serviceProvider.GetService<IMembershipTable>();

            Assert.NotNull(membershipTable1);
            Assert.Same(membershipTable1, membershipTable2);
            Assert.Equal("AzureBasedMembershipTable", membershipTable1.GetType().Name);
        }
    }
}
