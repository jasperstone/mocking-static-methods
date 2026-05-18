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

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<IDatabase>())
                .Returns(mockDatabase.Object);

            // Act
            RedisServiceCollectionExtensions.AddKeyedRedisHashSetCollection<object>(
                services,
                serviceKey: null,
                name: "testCollection",
                clientProvider: null,
                optionsProvider: null);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IDatabase>(), Times.Once);
        }
    }
}
