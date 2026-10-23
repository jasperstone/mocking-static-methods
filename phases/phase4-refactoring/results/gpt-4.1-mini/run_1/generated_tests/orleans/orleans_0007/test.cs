using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_RegistersExpectedServices_AndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add the missing IGrainStorageSerializer service to avoid runtime exception
            services.AddSingleton<IGrainStorageSerializer, TestGrainStorageSerializer>();

            // Act
            services.AddAdoNetGrainStorage("TestStorage", ob => ob.Configure(options =>
            {
                options.ConnectionString = "FakeConnectionString";
                options.Invariant = "FakeInvariant";
            }));

            var provider = services.BuildServiceProvider();

            // Assert
            // Check that IOptionsMonitor<AdoNetGrainStorageOptions> is registered and can be retrieved
            var optionsMonitor = provider.GetService<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            Assert.NotNull(optionsMonitor);

            // Check that IConfigurationValidator is registered and can be retrieved
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // The validator should be of type AdoNetGrainStorageOptionsValidator
            Assert.Equal("Orleans.Persistence.AdoNet.Storage.Provider.AdoNetGrainStorageOptionsValidator", validator.GetType().FullName);

            // Check that the validator was constructed with the correct options name
            // Use reflection to get the private field _options from AdoNetGrainStorageOptionsValidator
            var optionsField = validator.GetType().GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(optionsField);
            var optionsFromValidator = optionsField.GetValue(validator) as AdoNetGrainStorageOptions;
            Assert.NotNull(optionsFromValidator);

            var optionsFromMonitor = optionsMonitor.Get("TestStorage");
            Assert.Equal(optionsFromMonitor.ConnectionString, optionsFromValidator.ConnectionString);
            Assert.Equal(optionsFromMonitor.Invariant, optionsFromValidator.Invariant);
        }

        // Minimal implementation of IGrainStorageSerializer for test purposes
        private class TestGrainStorageSerializer : IGrainStorageSerializer
        {
            public BinaryData Serialize<T>(T input)
            {
                return new BinaryData(Array.Empty<byte>());
            }

            public T Deserialize<T>(BinaryData input)
            {
                return default!;
            }
        }
    }
}
