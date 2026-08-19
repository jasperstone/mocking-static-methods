using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using Microsoft.SemanticKernel.Connectors.Redis;
using Microsoft.Extensions.VectorData;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedRedisHashSetCollection_ShouldRegisterServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDatabase = new Mock<IDatabase>();

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IDatabase>())
                .Returns(mockDatabase.Object);

            serviceCollection.AddSingleton(mockServiceProvider.Object);

            // Act
            serviceCollection.AddKeyedRedisHashSetCollection<TestRecord>(
                serviceKey: "testKey",
                name: "testCollection",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var redisHashSetCollection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>("testKey");
            Assert.NotNull(redisHashSetCollection);
            Assert.Equal("testCollection", redisHashSetCollection.Name);
        }

        private class TestRecord
        {
        }
    }
}
