using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_RegistersExpectedServices()
        {
            var services = new ServiceCollection();

            // Register a dummy IGrainStorageSerializer to satisfy the dependency
            services.AddSingleton<IGrainStorageSerializer, DummyGrainStorageSerializer>();

            // Call the method under test
            services.AddDynamoDBGrainStorage("TestStorage", optionsBuilder =>
            {
                optionsBuilder.Configure(opts =>
                {
                    opts.Service = "dummy";
                });
            });

            var provider = services.BuildServiceProvider();

            // Assert that IConfigurationValidator is registered and can be resolved
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            // Assert that IPostConfigureOptions<DynamoDBStorageOptions> is registered
            var postConfigure = provider.GetService<IPostConfigureOptions<DynamoDBStorageOptions>>();
            Assert.NotNull(postConfigure);

            // Assert that the named options monitor can be resolved and returns the configured options
            var optionsMonitor = provider.GetService<IOptionsMonitor<DynamoDBStorageOptions>>();
            Assert.NotNull(optionsMonitor);
            var options = optionsMonitor.Get("TestStorage");
            Assert.NotNull(options);
            Assert.Equal("dummy", options.Service);
        }

        private class DummyGrainStorageSerializer : IGrainStorageSerializer
        {
            public BinaryData Serialize<T>(T input) => BinaryData.FromString("dummy");
            public T Deserialize<T>(BinaryData input) => default!;
        }
    }
}
