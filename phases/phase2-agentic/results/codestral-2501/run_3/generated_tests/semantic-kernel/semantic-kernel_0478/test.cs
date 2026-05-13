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

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IDatabase>())
                .Returns(mockDatabase.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<object>(serviceKey, name, clientProvider: sp => mockDatabase.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var redisHashSetCollection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, object>>(serviceKey);
            var vectorStoreCollection = serviceProvider.GetRequiredKeyedService<VectorStoreCollection<string, object>>(serviceKey);
            var vectorSearchable = serviceProvider.GetRequiredKeyedService<IVectorSearchable<object>>(serviceKey);

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

            // Act
            services.AddKeyedRedisHashSetCollection<object>(serviceKey, name, clientProvider: sp => mockDatabase.Object);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var redisHashSetCollection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, object>>(serviceKey);

            Assert.NotNull(redisHashSetCollection);
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

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IDatabase>())
                .Returns(mockDatabase.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<object>(serviceKey, name);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var redisHashSetCollection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, object>>(serviceKey);

            Assert.NotNull(redisHashSetCollection);
        }
    }
}
