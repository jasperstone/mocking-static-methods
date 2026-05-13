using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Redis;
using NSubstitute;
using StackExchange.Redis;
using Xunit;

namespace SemanticKernel.UnitTests.VectorData.Redis
{
    public class RedisServiceCollectionExtensionsTests
    {
        private sealed class TestRecord
        {
            public string? Id { get; set; }
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_UsesServiceProviderDatabase_WhenClientProviderIsNull()
        {
            var services = new ServiceCollection();
            var expectedDatabase = Substitute.For<IDatabase>();

            services.AddSingleton<IDatabase>(expectedDatabase);

            services.AddKeyedRedisHashSetCollection<TestRecord>(
                serviceKey: null,
                name: "collection",
                optionsProvider: _ => new RedisHashSetCollectionOptions());

            using var provider = services.BuildServiceProvider();
            var collection = provider.GetRequiredService<RedisHashSetCollection<string, TestRecord>>();

            Assert.NotNull(collection);

            var databaseField = typeof(RedisHashSetCollection<string, TestRecord>)
                .GetField("_database", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(databaseField);
            Assert.Same(expectedDatabase, databaseField!.GetValue(collection));
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_Throws_WhenDatabaseNotRegistered()
        {
            var services = new ServiceCollection();

            services.AddKeyedRedisHashSetCollection<TestRecord>(
                serviceKey: null,
                name: "collection",
                optionsProvider: _ => new RedisHashSetCollectionOptions());

            using var provider = services.BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(
                () => provider.GetRequiredService<RedisHashSetCollection<string, TestRecord>>());

            Assert.Contains(typeof(IDatabase).FullName!, exception.Message);
        }

        [Fact]
        public void AddKeyedRedisHashSetCollection_UsesProvidedClientProvider()
        {
            var services = new ServiceCollection();
            var expectedDatabase = Substitute.For<IDatabase>();

            services.AddKeyedRedisHashSetCollection<TestRecord>(
                serviceKey: null,
                name: "collection",
                clientProvider: sp =>
                {
                    Assert.NotNull(sp);
                    return expectedDatabase;
                },
                optionsProvider: _ => new RedisHashSetCollectionOptions());

            using var provider = services.BuildServiceProvider();
            var collection = provider.GetRequiredService<RedisHashSetCollection<string, TestRecord>>();

            var databaseField = typeof(RedisHashSetCollection<string, TestRecord>)
                .GetField("_database", BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.NotNull(databaseField);
            Assert.Same(expectedDatabase, databaseField!.GetValue(collection));
        }
    }
}
