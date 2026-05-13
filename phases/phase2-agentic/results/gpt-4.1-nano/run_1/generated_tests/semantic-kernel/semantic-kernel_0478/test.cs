using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace RedisServiceCollectionExtensionsTests
{
    public class AddKeyedRedisHashSetCollectionTests
    {
        [Fact]
        public void AddKeyedRedisHashSetCollection_Should_Call_GetRequiredService_When_ClientProvider_Is_Null()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new ServiceCollection()
                .BuildServiceProvider();

            var databaseMock = new Mock<IDatabase>();
            var servicesMock = new ServiceCollection();

            // Setup a mock for IServiceProvider to return IDatabase
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetRequiredService<IDatabase>())
                .Returns(databaseMock.Object);

            // Act
            services.AddKeyedRedisHashSetCollection<string>(
                serviceKey: "testKey",
                name: "testName",
                clientProvider: null,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Retrieve the service to trigger the registration
            var service = provider.GetService<RedisHashSetCollection<string, string>>();

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_Should_Use_ClientProvider_When_Provided()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockClient = new Mock<IDatabase>();
            Func<IServiceProvider, IDatabase> clientProvider = sp => mockClient.Object;

            // Act
            services.AddKeyedRedisHashSetCollection<string>(
                serviceKey: "testKey",
                name: "testName",
                clientProvider: clientProvider,
                optionsProvider: null,
                lifetime: ServiceLifetime.Singleton);

            var provider = services.BuildServiceProvider();

            // Retrieve the service
            var service = provider.GetService<RedisHashSetCollection<string, string>>();

            // Assert
            Assert.NotNull(service);
        }
    }
}
