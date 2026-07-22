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

            bool clientProviderCalled = false;
            Func<IServiceProvider, IDatabase> clientProvider = sp =>
            {
                clientProviderCalled = true;
                return mockDatabase.Object;
            };

            // Act
            services.AddKeyedRedisHashSetCollection<object>(
                serviceKey: null,
                name: "test",
                clientProvider: clientProvider,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the RedisHashSetCollection<string, object> to trigger the factory
            var collection = serviceProvider.GetRequiredService<RedisHashSetCollection<string, object>>();

            // Assert
            Assert.True(clientProviderCalled);
            Assert.NotNull(collection);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_UsesGetRequiredService_WhenClientProviderIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDatabase = new Mock<IDatabase>();
            services.AddSingleton(mockDatabase.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<object>(
                serviceKey: null,
                name: "test",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            var serviceProvider = services.BuildServiceProvider();

            // Resolve the RedisHashSetCollection<string, object> to trigger the factory
            var collection = serviceProvider.GetRequiredService<RedisHashSetCollection<string, object>>();

            // Assert
            Assert.NotNull(collection);
        }
    }
}
