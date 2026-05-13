using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Extensions.VectorData.Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddKeyedRedisHashSetCollection_AddsRedisHashSetCollectionToServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var clientProvider = new Mock<Func<IServiceProvider, IDatabase>>();
            var optionsProvider = new Mock<Func<IServiceProvider, RedisHashSetCollectionOptions>>();
            var name = "TestCollection";

            // Act
            services.AddKeyedRedisHashSetCollection<string>(serviceKey: null, name, clientProvider.Object, optionsProvider.Object);

            // Assert
            var serviceProviderAfterAdd = services.BuildServiceProvider();
            var redisHashSetCollection = serviceProviderAfterAdd.GetService<RedisHashSetCollection<string, string>>();
            Assert.NotNull(redisHashSetCollection);
        }

        [Fact]
        public async Task AddKeyedRedisHashSetCollection_UsesClientProviderIfProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var clientProvider = new Mock<Func<IServiceProvider, IDatabase>>();
            clientProvider.Setup(provider => provider(It.IsAny<IServiceProvider>())).Returns(new Mock<IDatabase>().Object);
            var optionsProvider = new Mock<Func<IServiceProvider, RedisHashSetCollectionOptions>>();
            var name = "TestCollection";

            // Act
            services.AddKeyedRedisHashSetCollection<string>(serviceKey: null, name, clientProvider.Object, optionsProvider.Object);

            // Assert
            var serviceProviderAfterAdd = services.BuildServiceProvider();
            var redisHashSetCollection = serviceProviderAfterAdd.GetService<RedisHashSetCollection<string, string>>();
            Assert.NotNull(redisHashSetCollection);
            clientProvider.Verify(provider => provider(It.IsAny<IServiceProvider>()), Times.Once);
        }

        [Fact]
        public async Task AddKeyedRedisHashSetCollection_UsesGetRequiredServiceIfNoClientProviderIsProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var database = new Mock<IDatabase>();
            services.AddSingleton<IDatabase>(database.Object);
            var optionsProvider = new Mock<Func<IServiceProvider, RedisHashSetCollectionOptions>>();
            var name = "TestCollection";

            // Act
            services.AddKeyedRedisHashSetCollection<string>(serviceKey: null, name, null, optionsProvider.Object);

            // Assert
            var serviceProviderAfterAdd = services.BuildServiceProvider();
            var redisHashSetCollection = serviceProviderAfterAdd.GetService<RedisHashSetCollection<string, string>>();
            Assert.NotNull(redisHashSetCollection);
            database.Verify(db => db, Times.Once);
        }
    }
}
