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
        public void AddKeyedRedisHashSetCollection_Should_Register_Service_And_Call_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDatabase = new Mock<IDatabase>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Setup IServiceProvider to return the mock IDatabase when requested
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IDatabase>())
                .Returns(mockDatabase.Object);

            // Act
            // Call the extension method with a dummy TRecord type
            services.AddKeyedRedisHashSetCollection<SampleRecord>(
                serviceKey: "testKey",
                name: "testCollection",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Retrieve the service to trigger registration
            var collection = provider.GetService<RedisHashSetCollection<string, SampleRecord>>();

            // Assert
            Assert.NotNull(collection);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IDatabase>(), Times.AtLeastOnce);
        }

        private class SampleRecord
        {
        }
    }
}
