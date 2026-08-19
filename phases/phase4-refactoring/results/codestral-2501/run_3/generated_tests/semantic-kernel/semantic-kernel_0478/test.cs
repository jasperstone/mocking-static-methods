using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
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
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<IDatabase>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IDatabase)))
                .Returns(databaseMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IDatabase>())
                .Returns(databaseMock.Object);

            // Act
            serviceCollection.AddKeyedRedisHashSetCollection<TestRecord>(
                serviceKey: "testKey",
                name: "testCollection",
                clientProvider: sp => databaseMock.Object,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Assert
            var redisHashSetCollection = serviceProvider.GetRequiredKeyedService<RedisHashSetCollection<string, TestRecord>>("testKey");
            Assert.NotNull(redisHashSetCollection);
            Assert.Equal("testCollection", redisHashSetCollection.Name);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_ShouldUseServiceProvider_WhenClientProviderIsNull()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<IDatabase>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IDatabase)))
                .Returns(databaseMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<IDatabase>())
                .Returns(databaseMock.Object);

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
