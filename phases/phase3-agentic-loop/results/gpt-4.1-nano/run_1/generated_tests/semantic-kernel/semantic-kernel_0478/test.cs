using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedRedisHashSetCollection_Should_Call_GetRequiredService_When_ClientProvider_Is_Null()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a dummy IDatabase service to the service collection
            var databaseMock = new Mock<IDatabase>();
            services.AddSingleton<IDatabase>(databaseMock.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<SampleRecord>(
                serviceKey: "key",
                name: "testCollection",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Retrieve the registered service
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(RedisHashSetCollection<string, SampleRecord>));
            Assert.NotNull(descriptor);
        }
    }

    public class SampleRecord
    {
        public string Id { get; set; }
    }
}
