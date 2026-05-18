using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using Xunit;
using Microsoft.SemanticKernel.Connectors.Redis;

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

            // clientProvider returns the mockDatabase
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

            // Resolve the RedisHashSetCollection<string, DummyRecord> service
            var collection = serviceProvider.GetRequiredService<RedisHashSetCollection<string, DummyRecord>>();

            // Assert
            Assert.NotNull(collection);
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

            // Resolve the RedisHashSetCollection<string, DummyRecord> service
            var collection = serviceProvider.GetRequiredService<RedisHashSetCollection<string, DummyRecord>>();

            // Assert
            Assert.NotNull(collection);
        }
    }
}
