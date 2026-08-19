using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Redis;
using StackExchange.Redis;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedRedisHashSetCollection_UsesClientProvider_WhenProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDatabase = new Mock<IDatabase>();
            var clientProviderCalled = false;

            Func<IServiceProvider, IDatabase> clientProvider = sp =>
            {
                clientProviderCalled = true;
                return mockDatabase.Object;
            };

            // Act
            services.AddKeyedRedisHashSetCollection<object>(
                serviceKey: null,
                name: "testCollection",
                clientProvider: clientProvider,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            var provider = services.BuildServiceProvider();

            // Assert
            var collection = provider.GetRequiredService<RedisHashSetCollection<string, object>>();
            Assert.NotNull(collection);
            Assert.True(clientProviderCalled);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_UsesServiceProviderGetRequiredService_WhenClientProviderIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDatabase = new Mock<IDatabase>();

            // Register IDatabase in the service collection so GetRequiredService<IDatabase>() can resolve it
            services.AddSingleton(mockDatabase.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<object>(
                serviceKey: null,
                name: "testCollection",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            var provider = services.BuildServiceProvider();

            // Assert
            var collection = provider.GetRequiredService<RedisHashSetCollection<string, object>>();
            Assert.NotNull(collection);
        }
    }
}
