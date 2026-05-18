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
        public void AddKeyedRedisHashSetCollection_RegistersRedisHashSetCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var clientProvider = new Func<IServiceProvider, IDatabase>(sp => Mock.Of<IDatabase>());
            var optionsProvider = new Func<IServiceProvider, RedisHashSetCollectionOptions>(sp => new RedisHashSetCollectionOptions());

            // Act
            services.AddKeyedRedisHashSetCollection<object>(null, "test", clientProvider, optionsProvider);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var redisHashSetCollection = serviceProvider.GetService(typeof(RedisHashSetCollection<string, object>));
            Assert.NotNull(redisHashSetCollection);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_RegistersVectorStoreCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var clientProvider = new Func<IServiceProvider, IDatabase>(sp => Mock.Of<IDatabase>());
            var optionsProvider = new Func<IServiceProvider, RedisHashSetCollectionOptions>(sp => new RedisHashSetCollectionOptions());

            // Act
            services.AddKeyedRedisHashSetCollection<object>(null, "test", clientProvider, optionsProvider);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var vectorStoreCollection = serviceProvider.GetService(typeof(VectorStoreCollection<string, object>));
            Assert.NotNull(vectorStoreCollection);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_RegistersIVectorSearchable()
        {
            // Arrange
            var services = new ServiceCollection();
            var clientProvider = new Func<IServiceProvider, IDatabase>(sp => Mock.Of<IDatabase>());
            var optionsProvider = new Func<IServiceProvider, RedisHashSetCollectionOptions>(sp => new RedisHashSetCollectionOptions());

            // Act
            services.AddKeyedRedisHashSetCollection<object>(null, "test", clientProvider, optionsProvider);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var vectorSearchable = serviceProvider.GetService(typeof(IVectorSearchable<object>));
            Assert.NotNull(vectorSearchable);
        }
    }
}
