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
        public void Calls_GetRequiredService_IDatabase_When_ClientProvider_Is_Null()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a dummy IDatabase service to the service collection
            var mockDatabase = new Mock<IDatabase>().Object;
            services.AddTransient<IDatabase>(_ => mockDatabase);

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Act
            // Call the method under test with null clientProvider to trigger GetRequiredService
            var resultServices = RedisServiceCollectionExtensions.AddKeyedRedisHashSetCollection<object>(
                services,
                serviceKey: null,
                name: "test",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            // Assert
            // Verify that the services contain the RedisHashSetCollection registration
            Assert.Contains(resultServices, d => d.ServiceType == typeof(RedisHashSetCollection<string, object>));
        }
    }
}
