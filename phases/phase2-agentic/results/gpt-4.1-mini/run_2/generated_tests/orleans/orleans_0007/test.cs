using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Storage;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_ShouldRegisterServices_AndCallGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var returnedServices = services.AddAdoNetGrainStorage("TestStorage", ob => ob.Configure(options =>
            {
                options.ConnectionString = "FakeConnectionString";
                options.Invariant = "FakeInvariant";
            }));

            // Assert
            Assert.Same(services, returnedServices);

            // Build service provider to test the transient registrations
            var serviceProvider = services.BuildServiceProvider();

            // The IConfigurationValidator registration uses GetRequiredService on IOptionsMonitor<AdoNetGrainStorageOptions>
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // Also check that the IOptionsMonitor<AdoNetGrainStorageOptions> is registered and can get the named options
            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            Assert.NotNull(optionsMonitor);

            var options = optionsMonitor.Get("TestStorage");
            Assert.NotNull(options);
            Assert.Equal("FakeConnectionString", options.ConnectionString);
            Assert.Equal("FakeInvariant", options.Invariant);
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_ShouldCallAddAdoNetGrainStorageWithDefaultName()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var returnedServices = services.AddAdoNetGrainStorageAsDefault();

            // Assert
            Assert.Same(services, returnedServices);

            // Build service provider to test the transient registrations
            var serviceProvider = services.BuildServiceProvider();

            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            Assert.NotNull(optionsMonitor);

            var options = optionsMonitor.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);
            Assert.NotNull(options);
        }
    }
}
