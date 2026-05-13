using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedRedisHashSetCollection_WhenClientProviderIsNull_UsesGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDatabase = new Mock<IDatabase>();
            services.AddSingleton<IDatabase>(mockDatabase.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            RedisServiceCollectionExtensions.AddKeyedRedisHashSetCollection<object>(
                services,
                serviceKey: null,
                name: "testCollection",
                clientProvider: null,
                optionsProvider: null);

            var provider = services.BuildServiceProvider();

            // Assert
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(RedisHashSetCollection<string, object>));
            Assert.NotNull(descriptor);

            var factory = (Func<IServiceProvider, RedisHashSetCollection<string, object>>)descriptor.ImplementationFactory;
            var collection = factory(provider);

            mockDatabase.Verify(db => db.Execute("PING"), Times.Once);
        }
    }
}
