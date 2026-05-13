using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.VectorData;
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
            var mockServiceProvider = new Mock<IServiceProvider>();

            // clientProvider returns the mockDatabase
            Func<IServiceProvider, IDatabase> clientProvider = sp => mockDatabase.Object;

            // optionsProvider returns default options
            Func<IServiceProvider, RedisHashSetCollectionOptions> optionsProvider = sp => new RedisHashSetCollectionOptions();

            // Act
            services.AddKeyedRedisHashSetCollection<DummyRecord>(
                serviceKey: null,
                name: "testCollection",
                clientProvider: clientProvider,
                optionsProvider: optionsProvider,
                lifetime: ServiceLifetime.Singleton);

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Resolve the RedisHashSetCollection<string, DummyRecord>
            var collection = serviceProvider.GetRequiredService<RedisHashSetCollection<string, DummyRecord>>();

            // Assert
            Assert.NotNull(collection);
            Assert.Equal("testCollection", collection.Name);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_UsesServiceProviderGetRequiredService_WhenClientProviderIsNull()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDatabase = new Mock<IDatabase>();

            // Register IDatabase in the service collection
            services.AddSingleton(mockDatabase.Object);

            // clientProvider is null, so the extension method should call sp.GetRequiredService<IDatabase>()
            Func<IServiceProvider, IDatabase>? clientProvider = null;

            Func<IServiceProvider, RedisHashSetCollectionOptions> optionsProvider = sp => new RedisHashSetCollectionOptions();

            // Act
            services.AddKeyedRedisHashSetCollection<DummyRecord>(
                serviceKey: null,
                name: "testCollection",
                clientProvider: clientProvider,
                optionsProvider: optionsProvider,
                lifetime: ServiceLifetime.Singleton);

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the RedisHashSetCollection<string, DummyRecord>
            var collection = serviceProvider.GetRequiredService<RedisHashSetCollection<string, DummyRecord>>();

            // Assert
            Assert.NotNull(collection);
            Assert.Equal("testCollection", collection.Name);
        }
    }
}
