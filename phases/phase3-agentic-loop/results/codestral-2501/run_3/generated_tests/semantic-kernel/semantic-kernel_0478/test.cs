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
            serviceCollection.AddSingleton(databaseMock.Object);

            // Act
            serviceCollection.AddKeyedRedisHashSetCollection<TestRecord>(serviceKey: "testKey", name: "testCollection", clientProvider: sp => databaseMock.Object);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
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
