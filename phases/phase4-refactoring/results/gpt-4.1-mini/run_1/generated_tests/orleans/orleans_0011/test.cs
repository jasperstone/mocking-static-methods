using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Runtime.MembershipService;
using Xunit;

namespace Orleans.Clustering.AzureStorage.Tests
{
    public class AzureTableClusteringExtensionsTests
    {
        [Fact]
        public void UseAzureStorageClustering_WithOptionsBuilder_RegistersConfigurationValidator()
        {
            var services = new ServiceCollection();

            var builder = new TestSiloBuilder(services);

            // Call the extension method with a configureOptions action that does nothing
            AzureTableClusteringExtensions.UseAzureStorageClustering(builder, optionsBuilder =>
            {
                // No configuration to avoid unknown properties
            });

            var serviceProvider = services.BuildServiceProvider();

            // The IConfigurationValidator should be registered as transient
            var validator1 = serviceProvider.GetService<IConfigurationValidator>();
            var validator2 = serviceProvider.GetService<IConfigurationValidator>();

            Assert.NotNull(validator1);
            Assert.NotNull(validator2);
            Assert.NotSame(validator1, validator2);

            // The validator should be of type AzureStorageClusteringOptionsValidator
            Assert.Equal("AzureStorageClusteringOptionsValidator", validator1.GetType().Name);

            // The IMembershipTable should be registered as singleton and of type AzureBasedMembershipTable
            var membershipTable1 = serviceProvider.GetService<IMembershipTable>();
            var membershipTable2 = serviceProvider.GetService<IMembershipTable>();

            Assert.NotNull(membershipTable1);
            Assert.NotNull(membershipTable2);
            Assert.Same(membershipTable1, membershipTable2);
            Assert.Equal("AzureBasedMembershipTable", membershipTable1.GetType().Name);
        }

        private interface ISiloBuilder
        {
            ISiloBuilder ConfigureServices(Action<IServiceCollection> configureDelegate);
        }

        private class TestSiloBuilder : ISiloBuilder
        {
            public IServiceCollection Services { get; }

            public TestSiloBuilder(IServiceCollection services)
            {
                Services = services;
            }

            public ISiloBuilder ConfigureServices(Action<IServiceCollection> configureDelegate)
            {
                configureDelegate(Services);
                return this;
            }
        }
    }
}
