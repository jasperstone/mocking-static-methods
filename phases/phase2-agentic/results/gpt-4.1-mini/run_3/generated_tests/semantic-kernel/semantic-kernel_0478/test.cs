using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        private class DummyRecord { }

        [Fact]
        public void AddKeyedRedisHashSetCollection_UsesClientProvider_WhenProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDatabase = new Mock<IDatabase>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Setup clientProvider to return mockDatabase
            Func<IServiceProvider, IDatabase> clientProvider = sp => mockDatabase.Object;

            // Act
            services.AddKeyedRedisHashSetCollection<DummyRecord>(
                serviceKey: null,
                name: "testCollection",
                clientProvider: clientProvider,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Resolve RedisHashSetCollection<string, DummyRecord>
            var collection = serviceProvider.GetRequiredService<RedisHashSetCollection<string, DummyRecord>>();

            // Assert
            Assert.NotNull(collection);
            // The client inside collection should be the mockDatabase
            Assert.Same(mockDatabase.Object, GetClientFromCollection(collection));
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_UsesServiceProviderGetRequiredService_WhenClientProviderIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDatabase = new Mock<IDatabase>();

            // Register IDatabase in the service collection
            services.AddSingleton(mockDatabase.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<DummyRecord>(
                serviceKey: null,
                name: "testCollection",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Resolve RedisHashSetCollection<string, DummyRecord>
            var collection = serviceProvider.GetRequiredService<RedisHashSetCollection<string, DummyRecord>>();

            // Assert
            Assert.NotNull(collection);
            // The client inside collection should be the mockDatabase from service provider
            Assert.Same(mockDatabase.Object, GetClientFromCollection(collection));
        }

        private static IDatabase GetClientFromCollection(RedisHashSetCollection<string, DummyRecord> collection)
        {
            // Use reflection to get the private field _client or property Client
            var clientField = typeof(RedisHashSetCollection<string, DummyRecord>).GetField("_client", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (clientField != null)
            {
                return (IDatabase)clientField.GetValue(collection);
            }
            // If no field, try property
            var clientProperty = typeof(RedisHashSetCollection<string, DummyRecord>).GetProperty("Client", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (clientProperty != null)
            {
                return (IDatabase)clientProperty.GetValue(collection);
            }
            throw new InvalidOperationException("Could not find client field or property on RedisHashSetCollection.");
        }
    }
}
