using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.AdoNet.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_WithNameAndConfigureOptions_RegistersServicesAndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddAdoNetGrainStorage("TestStorage", ob => ob.Configure(options =>
            {
                options.Invariant = "System.Data.SqlClient";
                options.ConnectionString = "FakeConnectionString";
            }));

            // Assert
            Assert.Same(services, result);

            // Build service provider to test the factory delegate that calls GetRequiredService
            var serviceProvider = services.BuildServiceProvider();

            // The IConfigurationValidator service should be registered and resolvable
            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // The validator should be of type AdoNetGrainStorageOptionsValidator
            Assert.IsType<AdoNetGrainStorageOptionsValidator>(validator);
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_UsesDefaultNameAndRegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddAdoNetGrainStorageAsDefault();

            // Assert
            Assert.Same(services, result);

            var serviceProvider = services.BuildServiceProvider();

            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AdoNetGrainStorageOptionsValidator>(validator);
        }

        [Fact]
        public void AddAdoNetGrainStorage_WithConfigureOptionsAction_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddAdoNetGrainStorage("MyStorage", (Action<AdoNetGrainStorageOptions>)(opts =>
            {
                opts.Invariant = "System.Data.SqlClient";
                opts.ConnectionString = "FakeConnectionString";
            }));

            // Assert
            Assert.Same(services, result);

            var serviceProvider = services.BuildServiceProvider();

            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AdoNetGrainStorageOptionsValidator>(validator);
        }
    }
}
