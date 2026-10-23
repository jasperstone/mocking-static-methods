using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        private class FakeGrainStorageSerializer : IGrainStorageSerializer
        {
            public BinaryData Serialize<T>(T input) => new BinaryData(Array.Empty<byte>());
            public T Deserialize<T>(BinaryData input) => default!;
        }

        [Fact]
        public void AddAdoNetGrainStorage_WithNameAndConfigureOptions_RegistersServicesAndValidator()
        {
            // Arrange
            var services = new ServiceCollection();
            var storageName = "TestStorage";

            // Register a fake IGrainStorageSerializer to satisfy dependency
            services.AddSingleton<IGrainStorageSerializer, FakeGrainStorageSerializer>();

            // Act
            var returnedServices = services.AddAdoNetGrainStorage(storageName, ob => ob.Configure(options =>
            {
                options.ConnectionString = "FakeConnectionString";
                options.Invariant = AdoNetGrainStorageOptions.DEFAULT_ADONET_INVARIANT;
                options.HashPicker = new StorageHasherPicker(new[] { new OrleansDefaultHasher() });
            }));

            // Assert
            Assert.Same(services, returnedServices);

            // Build service provider to test the service registrations
            var provider = services.BuildServiceProvider();

            // The IConfigurationValidator should be registered and resolvable
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AdoNetGrainStorageOptionsValidator>(validator);

            // The validator should have the correct name and options
            var optionsMonitor = provider.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var options = optionsMonitor.Get(storageName);
            Assert.Equal("FakeConnectionString", options.ConnectionString);
            Assert.Equal(AdoNetGrainStorageOptions.DEFAULT_ADONET_INVARIANT, options.Invariant);
            Assert.NotNull(options.HashPicker);
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_UsesDefaultStorageProviderName()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register a fake IGrainStorageSerializer to satisfy dependency
            services.AddSingleton<IGrainStorageSerializer, FakeGrainStorageSerializer>();

            // Also register default options for the default storage provider name to avoid missing IOptionsMonitor
            services.AddOptions<AdoNetGrainStorageOptions>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)
                .Configure(options =>
                {
                    options.ConnectionString = "DefaultConnectionString";
                    options.Invariant = AdoNetGrainStorageOptions.DEFAULT_ADONET_INVARIANT;
                    options.HashPicker = new StorageHasherPicker(new[] { new OrleansDefaultHasher() });
                });

            // Act
            var returnedServices = services.AddAdoNetGrainStorageAsDefault();

            // Assert
            Assert.Same(services, returnedServices);

            // Build service provider to test the service registrations
            var provider = services.BuildServiceProvider();

            // The IConfigurationValidator should be registered and resolvable
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<AdoNetGrainStorageOptionsValidator>(validator);
        }

        [Fact]
        public void AddAdoNetGrainStorage_WithConfigureOptionsAction_InvokesConfigureOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var called = false;

            // Act
            services.AddAdoNetGrainStorage("Test", ob =>
            {
                called = true;
                ob.Configure(options =>
                {
                    options.ConnectionString = "Conn";
                    options.Invariant = AdoNetGrainStorageOptions.DEFAULT_ADONET_INVARIANT;
                    options.HashPicker = new StorageHasherPicker(new[] { new OrleansDefaultHasher() });
                });
            });

            // Assert
            Assert.True(called);
        }
    }
}
