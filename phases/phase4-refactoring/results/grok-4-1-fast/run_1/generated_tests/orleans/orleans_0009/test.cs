using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        private readonly IServiceCollection services;
        private readonly MockGrainStorageSerializer mockSerializer;

        public DynamoDBGrainStorageServiceCollectionExtensionsTests()
        {
            services = new ServiceCollection();
            mockSerializer = new MockGrainStorageSerializer();
            services.AddSingleton<IGrainStorageSerializer>(mockSerializer);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ReturnsSameServiceCollection()
        {
            // Act
            var result = services.AddDynamoDBGrainStorage("testName");

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_ReturnsSameServiceCollection()
        {
            // Act
            var result = services.AddDynamoDBGrainStorageAsDefault();

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_RegistersDynamoDBGrainStorageOptionsValidator()
        {
            // Act
            services.AddDynamoDBGrainStorage("testName");

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var validators = serviceProvider.GetServices<IConfigurationValidator>();
            Assert.Contains(validators, v => v.GetType().Name == "DynamoDBGrainStorageOptionsValidator");
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_RegistersDynamoDBGrainStorageOptionsValidator()
        {
            // Act
            services.AddDynamoDBGrainStorageAsDefault();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var validators = serviceProvider.GetServices<IConfigurationValidator>();
            Assert.Contains(validators, v => v.GetType().Name == "DynamoDBGrainStorageOptionsValidator");
        }

        [Fact]
        public void AddDynamoDBGrainStorage_WhenConfigureOptionsIsNull_DoesNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => services.AddDynamoDBGrainStorage("testName", (Action<OptionsBuilder<DynamoDBStorageOptions>>)null));
            Assert.Null(exception);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ConfiguresOptionsCorrectly()
        {
            // Arrange
            services.Clear();

            // Act
            services.AddDynamoDBGrainStorage("testName", opts => opts.Configure(o => o.TableName = "TestTable"));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>();
            var options = optionsMonitor.Get("testName");
            Assert.Equal("TestTable", options.TableName);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_ConfiguresOptionsCorrectly()
        {
            // Arrange
            services.Clear();

            // Act
            services.AddDynamoDBGrainStorageAsDefault(opts => opts.Configure(o => o.TableName = "TestTable"));

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<DynamoDBStorageOptions>>();
            var options = optionsMonitor.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);
            Assert.Equal("TestTable", options.TableName);
        }
    }

    public class MockGrainStorageSerializer : IGrainStorageSerializer
    {
        public void Serialize<T>(T value, IByteBuffer output) => throw new NotImplementedException();
        public T Deserialize<T>(IReadOnlyByteBuffer input) => throw new NotImplementedException();
    }
}
