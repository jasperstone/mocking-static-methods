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

            // Build service provider to invoke factory
            var serviceProvider = services.BuildServiceProvider();

            // Resolve RedisHashSetCollection<string, object> to trigger factory
            var collection = serviceProvider.GetService<RedisHashSetCollection<string, object>>();

            // Assert
            Assert.NotNull(collection);
            Assert.True(clientProviderCalled);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_UsesGetRequiredService_WhenClientProviderIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDatabase = new Mock<IDatabase>();

            // Register mockDatabase as IDatabase service
            services.AddSingleton(mockDatabase.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<object>(
                serviceKey: null,
                name: "test",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            // Build service provider to invoke factory
            var serviceProvider = services.BuildServiceProvider();

            // Resolve RedisHashSetCollection<string, object> to trigger factory
            var collection = serviceProvider.GetService<RedisHashSetCollection<string, object>>();

            // Assert
            Assert.NotNull(collection);
            // The client inside collection should be the mockDatabase instance
            Assert.Same(mockDatabase.Object, GetPrivateField<IDatabase>(collection, "_client"));
        }

        private static T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null)
            {
                throw new InvalidOperationException($"Field '{fieldName}' not found on type '{obj.GetType().FullName}'.");
            }
            return (T)field.GetValue(obj);
        }
    }
}
