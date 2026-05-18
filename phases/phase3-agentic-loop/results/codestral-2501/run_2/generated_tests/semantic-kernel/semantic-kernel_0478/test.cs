using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using Microsoft.SemanticKernel.Connectors.Redis;
using Microsoft.Extensions.VectorData;
using System;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedRedisHashSetCollection_ShouldRegisterServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var databaseMock = new Mock<IDatabase>();

            // Act
            serviceCollection.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: null, name: "testCollection", clientProvider: null, optionsProvider: null, lifetime: ServiceLifetime.Singleton);
            serviceCollection.AddSingleton(databaseMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var redisHashSetCollection = serviceProvider.GetRequiredService<RedisHashSetCollection<string, TestRecord>>();
            var vectorStoreCollection = serviceProvider.GetRequiredService<VectorStoreCollection<string, TestRecord>>();
            var vectorSearchable = serviceProvider.GetRequiredService<IVectorSearchable<TestRecord>>();

            Assert.NotNull(redisHashSetCollection);
            Assert.NotNull(vectorStoreCollection);
            Assert.NotNull(vectorSearchable);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_ShouldUseClientProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var databaseMock = new Mock<IDatabase>();

            // Act
            serviceCollection.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: null, name: "testCollection", clientProvider: sp => databaseMock.Object, optionsProvider: null, lifetime: ServiceLifetime.Singleton);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var redisHashSetCollection = serviceProvider.GetRequiredService<RedisHashSetCollection<string, TestRecord>>();
            var vectorStoreCollection = serviceProvider.GetRequiredService<VectorStoreCollection<string, TestRecord>>();
            var vectorSearchable = serviceProvider.GetRequiredService<IVectorSearchable<TestRecord>>();

            Assert.NotNull(redisHashSetCollection);
            Assert.NotNull(vectorStoreCollection);
            Assert.NotNull(vectorSearchable);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_ShouldUseOptionsProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var databaseMock = new Mock<IDatabase>();
            var options = new RedisHashSetCollectionOptions();

            // Act
            serviceCollection.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: null, name: "testCollection", clientProvider: null, optionsProvider: sp => options, lifetime: ServiceLifetime.Singleton);
            serviceCollection.AddSingleton(databaseMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
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
