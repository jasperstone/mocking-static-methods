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
        [Fact]
        public void UseAzureStorageClustering_WithOptionsBuilder_RegistersExpectedServices()
        {
            var services = new ServiceCollection();
            var builder = new TestSiloBuilder(services);

            // Call the extension method with a configureOptions action that sets a property
            builder.UseAzureStorageClustering(optionsBuilder =>
            {
                optionsBuilder.Configure(options =>
                {
                    options.ConnectionString = "UseDevelopmentStorage=true";
                });
            });

            var serviceProvider = services.BuildServiceProvider();

            // Assert that IOptionsMonitor<AzureStorageClusteringOptions> is registered and has the configured value
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AzureStorageClusteringOptions>>();
            var options = optionsMonitor.Get(Options.DefaultName);
            Assert.Equal("UseDevelopmentStorage=true", options.ConnectionString);

            // Assert that IConfigurationValidator is registered and is of expected type
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AzureStorageClusteringOptionsValidator>(validator);

            // Assert that IMembershipTable is registered and is AzureBasedMembershipTable
            var membershipTable = serviceProvider.GetRequiredService<IMembershipTable>();
            Assert.NotNull(membershipTable);
            Assert.IsType<AzureBasedMembershipTable>(membershipTable);
        }

        // Minimal test implementation of ISiloBuilder to support ConfigureServices
        private class TestSiloBuilder : ISiloBuilder
        {
            private readonly IServiceCollection _services;

            public TestSiloBuilder(IServiceCollection services)
            {
                _services = services;
            }

            public IServiceCollection Services => _services;

            public ISiloBuilder ConfigureServices(Action<IServiceCollection> configureDelegate)
            {
                configureDelegate(_services);
                return this;
            }
        }
    }

    // Minimal ISiloBuilder interface for testing
    public interface ISiloBuilder
    {
        IServiceCollection Services { get; }
        ISiloBuilder ConfigureServices(Action<IServiceCollection> configureDelegate);
    }
}
