using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Redis;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedRedisHashSetCollection_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceKey = "testKey";
            var name = "testCollection";
            var mockDatabase = new Mock<IDatabase>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IDatabase>()).Returns(mockDatabase.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey, name, clientProvider: null, optionsProvider: null, lifetime: ServiceLifetime.Singleton);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var redisHashSetCollection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>(serviceKey);
            var vectorStoreCollection = serviceProvider.GetRequiredKeyedService<VectorStoreCollection<string, TestRecord>>(serviceKey);
            var vectorSearchable = serviceProvider.GetRequiredKeyedService<IVectorSearchable<TestRecord>>(serviceKey);

            Assert.NotNull(redisHashSetCollection);
            Assert.NotNull(vectorStoreCollection);
            Assert.NotNull(vectorSearchable);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_ShouldUseClientProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceKey = "testKey";
            var name = "testCollection";
            var mockDatabase = new Mock<IDatabase>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            Func<IServiceProvider, IDatabase> clientProvider = sp => mockDatabase.Object;

            // Act
            services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey, name, clientProvider, optionsProvider: null, lifetime: ServiceLifetime.Singleton);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var redisHashSetCollection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>(serviceKey);
            var vectorStoreCollection = serviceProvider.GetRequiredKeyedService<VectorStoreCollection<string, TestRecord>>(serviceKey);
            var vectorSearchable = serviceProvider.GetRequiredKeyedService<IVectorSearchable<TestRecord>>(serviceKey);

            Assert.NotNull(redisHashSetCollection);
            Assert.NotNull(vectorStoreCollection);
            Assert.NotNull(vectorSearchable);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_ShouldUseOptionsProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceKey = "testKey";
            var name = "testCollection";
            var mockDatabase = new Mock<IDatabase>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            Func<IServiceProvider, RedisHashSetCollectionOptions> optionsProvider = sp => new RedisHashSetCollectionOptions();

            // Act
            services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey, name, clientProvider: null, optionsProvider, lifetime: ServiceLifetime.Singleton);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var redisHashSetCollection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>(serviceKey);
            var vectorStoreCollection = serviceProvider.GetRequiredKeyedService<VectorStoreCollection<string, TestRecord>>(serviceKey);
            var vectorSearchable = serviceProvider.GetRequiredKeyedService<IVectorSearchable<TestRecord>>(serviceKey);

            Assert.NotNull(redisHashSetCollection);
            Assert.NotNull(vectorStoreCollection);
            Assert.NotNull(vectorSearchable);
        }

        [Fact]
        public void AddRedisHashSetCollection_ShouldRegisterServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "testCollection";
            var connectionConfiguration = "localhost";

            // Act
            services.AddRedisHashSetCollection<TestRecord>(name, connectionConfiguration, options: null, lifetime: ServiceLifetime.Singleton);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var redisHashSetCollection = serviceProvider.GetRequiredService<RedisHashSetCollection<string, TestRecord>>();
            var vectorStoreCollection = serviceProvider.GetRequiredService<VectorStoreCollection<string, TestRecord>>();
            var vectorSearchable = serviceProvider.GetRequiredService<IVectorSearchable<TestRecord>>();

            Assert.NotNull(redisHashSetCollection);
            Assert.NotNull(vectorStoreCollection);
            Assert.NotNull(vectorSearchable);
        }

        private class TestRecord
        {
        }
    }
}
