using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Storage;
using Xunit;

namespace Orleans.Persistence.DynamoDB.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorage_RegistersConfigurationValidator_AndResolvesIt()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add required options monitor service for IOptionsMonitor<DynamoDBStorageOptions>
            services.AddOptions();

            // Add a dummy IGrainStorageSerializer to satisfy dependency
            services.AddSingleton<IGrainStorageSerializer, DummyGrainStorageSerializer>();

            // Act
            services.AddDynamoDBGrainStorage("TestStorage", optionsBuilder =>
            {
                optionsBuilder.Configure(opts =>
                {
                    opts.Service = "http://localhost";
                    opts.TableName = "TestTable";
                });
            });

            var provider = services.BuildServiceProvider();

            // Assert
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
            Assert.IsType<DynamoDBGrainStorageOptionsValidator>(validator);
        }

        private class DummyGrainStorageSerializer : IGrainStorageSerializer
        {
            public BinaryData Serialize<T>(T input)
            {
                throw new NotImplementedException();
            }

            public T Deserialize<T>(BinaryData input)
            {
                throw new NotImplementedException();
            }
        }
    }
}
