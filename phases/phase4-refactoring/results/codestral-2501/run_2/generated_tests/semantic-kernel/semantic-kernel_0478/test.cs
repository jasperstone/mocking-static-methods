using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Redis;
using Microsoft.Extensions.VectorData;
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
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<IDatabase>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IDatabase)))
                .Returns(databaseMock.Object);

            // Act
            serviceCollection.AddKeyedRedisHashSetCollection<TestRecord>(
                serviceKey: null,
                name: "testCollection",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            // Assert
            var serviceProvider = serviceProviderMock.Object;
            var redisHashSetCollection = serviceProvider.GetRequiredService<RedisHashSetCollection<string, TestRecord>>();
            var vectorStoreCollection = serviceProvider.GetRequiredService<VectorStoreCollection<string, TestRecord>>();
            var vectorSearchable = serviceProvider.GetRequiredService<IVectorSearchable<TestRecord>>();

            Assert.NotNull(redisHashSetCollection);
            Assert.NotNull(vectorStoreCollection);
            Assert.NotNull(vectorSearchable);

            serviceProviderMock.Verify(sp => sp.GetService(typeof(IDatabase)), Times.Once);
        }

        private class TestRecord
        {
        }
    }
}
