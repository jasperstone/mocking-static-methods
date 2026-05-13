using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using StackExchange.Redis;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RedisServiceCollectionExtensionsTests
{
    public class AddKeyedRedisHashSetCollectionTests
    {
        [Fact]
        public void Calls_GetRequiredService_IDatabase_When_ClientProvider_Is_Null()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new ServiceCollection();
            var serviceProvider = serviceProviderMock.BuildServiceProvider();

            var databaseMock = new Mock<IDatabase>();
            var servicesMock = new ServiceCollection();
            servicesMock.AddSingleton<IDatabase>(databaseMock.Object);
            var sp = servicesMock.BuildServiceProvider();

            // Add a dummy service to the service collection
            services.AddSingleton<IDatabase>(databaseMock.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<string>(
                serviceKey: "testKey",
                name: "testName",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Retrieve the service
            var redisCollection = provider.GetService<RedisHashSetCollection<string, string>>();

            // Assert
            Assert.NotNull(redisCollection);
            Assert.IsType<RedisHashSetCollection<string, string>>(redisCollection);
        }
    }
}
