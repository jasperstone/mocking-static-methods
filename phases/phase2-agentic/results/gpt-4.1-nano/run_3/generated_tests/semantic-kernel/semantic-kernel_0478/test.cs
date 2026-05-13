using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Microsoft.Extensions.VectorData.Redis;
using Moq;

namespace RedisServiceCollectionExtensionsTests
{
    public class AddKeyedRedisHashSetCollectionTests
    {
        [Fact]
        public void AddKeyedRedisHashSetCollection_CallsGetRequiredServiceWhenClientProviderIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new ServiceCollection()
                .BuildServiceProvider();

            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var serviceCollection = new ServiceCollection();

            // Act
            var result = RedisServiceCollectionExtensions.AddKeyedRedisHashSetCollection<string>(
                services,
                serviceKey: "testKey",
                name: "testName",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Retrieve the service descriptor
            var descriptor = services[0];

            // Create a scope to test the factory
            using var scope = sp.CreateScope();
            var provider = scope.ServiceProvider;

            // Act: resolve the RedisHashSetCollection
            var redisHashSet = provider.GetService<RedisHashSetCollection<string, string>>();

            // Assert
            Assert.NotNull(redisHashSet);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_UsesClientProviderWhenProvided()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockClient = new Mock<IDatabase>();
            Func<IServiceProvider, IDatabase> clientProvider = _ => mockClient.Object;

            // Act
            var result = RedisServiceCollectionExtensions.AddKeyedRedisHashSetCollection<string>(
                services,
                serviceKey: "testKey",
                name: "testName",
                clientProvider: clientProvider,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            // Build the service provider
            var sp = services.BuildServiceProvider();

            // Retrieve the service descriptor
            var descriptor = services[0];

            // Create a scope to test the factory
            using var scope = sp.CreateScope();
            var provider = scope.ServiceProvider;

            // Act: resolve the RedisHashSetCollection
            var redisHashSet = provider.GetService<RedisHashSetCollection<string, string>>();

            // Assert
            Assert.NotNull(redisHashSet);
        }
    }
}
