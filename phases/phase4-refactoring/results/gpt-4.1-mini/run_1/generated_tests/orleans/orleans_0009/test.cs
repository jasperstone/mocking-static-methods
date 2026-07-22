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
        public void AddDynamoDBGrainStorage_RegistersConfigurationValidator_AndResolvesIt()
        {
            var services = new ServiceCollection();

            // Add required options monitor service for DynamoDBStorageOptions
            services.AddOptions();

            // Add a dummy IGrainStorageSerializer to satisfy the dependency of DefaultStorageProviderSerializerOptionsConfigurator
            services.AddSingleton<IGrainStorageSerializer, DummyGrainStorageSerializer>();

            // Call the extension method under test
            services.AddDynamoDBGrainStorage("TestStorage", optionsBuilder =>
            {
                optionsBuilder.Configure(opts =>
                {
                    opts.TableName = "TestTable";
                    opts.ReadCapacityUnits = 5;
                    opts.WriteCapacityUnits = 5;
                    opts.UseProvisionedThroughput = true;
                });
            });

            var provider = services.BuildServiceProvider();

            // Resolving IConfigurationValidator triggers the factory delegate that calls GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>
            var validator = provider.GetRequiredService<IConfigurationValidator>();

            Assert.NotNull(validator);
            Assert.IsType<DynamoDBGrainStorageOptionsValidator>(validator);
        }

        private class DummyGrainStorageSerializer : IGrainStorageSerializer
        {
            public BinaryData Serialize<T>(T input) => new BinaryData(Array.Empty<byte>());
            public T Deserialize<T>(BinaryData input) => default!;
        }
    }
}
