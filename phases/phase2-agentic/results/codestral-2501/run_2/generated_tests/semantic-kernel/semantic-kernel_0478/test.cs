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
            services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey, name, clientProvider: sp => mockDatabase.Object);

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
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IDatabase>()).Returns(mockDatabase.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey, name, clientProvider: sp => mockDatabase.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var redisHashSetCollection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>(serviceKey);

            Assert.NotNull(redisHashSetCollection);
            Assert.Same(mockDatabase.Object, redisHashSetCollection.GetType().GetField("_database", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(redisHashSetCollection));
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_ShouldUseServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceKey = "testKey";
            var name = "testCollection";
            var mockDatabase = new Mock<IDatabase>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IDatabase>()).Returns(mockDatabase.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey, name);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var redisHashSetCollection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>(serviceKey);

            Assert.NotNull(redisHashSetCollection);
            Assert.Same(mockDatabase.Object, redisHashSetCollection.GetType().GetField("_database", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(redisHashSetCollection));
        }

        private class TestRecord
        {
        }
    }
}
