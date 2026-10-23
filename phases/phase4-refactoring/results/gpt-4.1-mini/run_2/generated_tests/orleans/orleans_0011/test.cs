using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
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
            var services = new ServiceCollection();
            var builder = new TestSiloBuilder(services);

            // Call the extension method with a configureOptions action that sets a property
            builder.UseAzureStorageClustering(options =>
            {
                options.ConnectionString = "UseDevelopmentStorage=true";
            });

            var serviceProvider = services.BuildServiceProvider();

            // Validate that AzureBasedMembershipTable is registered as singleton IMembershipTable
            var membershipTable = serviceProvider.GetService<IMembershipTable>();
            Assert.NotNull(membershipTable);
            Assert.IsType<AzureBasedMembershipTable>(membershipTable);

            // Validate that AzureStorageClusteringOptions is configured with the expected value
            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<AzureStorageClusteringOptions>>();
            Assert.NotNull(optionsMonitor);
            var options = optionsMonitor.Get(Options.DefaultName);
            Assert.Equal("UseDevelopmentStorage=true", options.ConnectionString);

            // Validate that IConfigurationValidator is registered and can be resolved
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AzureStorageClusteringOptionsValidator>(validator);
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

            public IConfiguration Configuration => null;
        }
    }
}
