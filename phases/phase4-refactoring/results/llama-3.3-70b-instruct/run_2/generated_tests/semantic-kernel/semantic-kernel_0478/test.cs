using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Redis;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class RedisServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddKeyedRedisHashSetCollection_AddsCollectionToServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = "Test";
                options.Configuration = "localhost";
            });
            services.AddTransient<IDatabase>(provider => ConnectionMultiplexer.Connect("localhost").GetDatabase());
            var serviceProvider = services.BuildServiceProvider();
            var database = serviceProvider.GetRequiredService<IDatabase>();
            var options = new RedisHashSetCollectionOptions();

            // Act
            services.AddKeyedRedisHashSetCollection<string>(serviceKey: null, name: "test", clientProvider: _ => database, optionsProvider: _ => options);

            // Assert
            var collection = serviceProvider.GetService<VectorStoreCollection<string, string>>();
            Assert.NotNull(collection);
        }

        [Fact]
        public async Task AddKeyedRedisHashSetCollection_GetRequiredService_CallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = "Test";
                options.Configuration = "localhost";
            });
            services.AddTransient<IDatabase>(provider => ConnectionMultiplexer.Connect("localhost").GetDatabase());
            var serviceProvider = services.BuildServiceProvider();
            var database = serviceProvider.GetRequiredService<IDatabase>();
            var options = new RedisHashSetCollectionOptions();

            // Act
            services.AddKeyedRedisHashSetCollection<string>(serviceKey: null, name: "test", clientProvider: _ => database, optionsProvider: _ => options);

            // Assert
            var collection = serviceProvider.GetService<VectorStoreCollection<string, string>>();
            Assert.NotNull(collection);
        }
    }
}
