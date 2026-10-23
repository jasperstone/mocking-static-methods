using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedRedisHashSetCollection_ShouldResolveIDatabase_WhenClientProviderIsNull()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDatabase = new Mock<IDatabase>();
            services.AddSingleton(mockDatabase.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<SampleRecord>(
                serviceKey: "testKey",
                name: "testCollection",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            // Build service provider
            var provider = services.BuildServiceProvider();

            // Assert
            var db = provider.GetRequiredService<IDatabase>();
            Assert.NotNull(db);
        }

        public class SampleRecord
        {
            public string Id { get; set; }
        }
    }
}
