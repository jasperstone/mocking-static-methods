using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Moq;

namespace RedisServiceCollectionExtensionsTests
{
    public class AddKeyedRedisHashSetCollectionTests
    {
        [Fact]
        public void Calls_GetRequiredService_IDatabase_When_ClientProvider_Is_Null()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a mock IDatabase to the service collection
            var mockDatabase = new Mock<IDatabase>();
            services.AddSingleton<IDatabase>(mockDatabase.Object);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = RedisServiceCollectionExtensions.AddKeyedRedisHashSetCollection<string>(
                services,
                serviceKey: "testKey",
                name: "testName",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            // Build the final service provider to resolve services
            var sp = result.BuildServiceProvider();

            // Assert
            var database = sp.GetRequiredService<IDatabase>();
            Assert.NotNull(database);
            Assert.Equal(mockDatabase.Object, database);
        }
    }
}
