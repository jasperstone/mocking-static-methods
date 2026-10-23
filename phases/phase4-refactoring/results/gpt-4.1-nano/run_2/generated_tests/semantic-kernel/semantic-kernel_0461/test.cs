using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using MongoDB.Driver;
using Moq;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.AI;

namespace VectorData.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddingGenerator_WhenGeneratorIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator>(mockGenerator.Object);
            services.AddTransient<SomeOptions>(sp => new SomeOptions());

            var provider = services.BuildServiceProvider();

            // Act
            var options = MongoServiceCollectionExtensions.GetStoreOptions(provider, sp => new SomeOptions());

            // Assert
            Assert.NotNull(options);
            Assert.Equal(mockGenerator.Object, options.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<SomeOptions>(sp => new SomeOptions());

            var provider = services.BuildServiceProvider();

            // Act
            var options = MongoServiceCollectionExtensions.GetStoreOptions(provider, sp => new SomeOptions());

            // Assert
            Assert.NotNull(options);
            Assert.Null(options.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithEmbeddingGenerator_WhenGeneratorIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator>(mockGenerator.Object);
            services.AddTransient<SomeOptions>(sp => new SomeOptions());

            var provider = services.BuildServiceProvider();

            // Act
            var options = MongoServiceCollectionExtensions.GetCollectionOptions(provider, sp => new SomeOptions());

            // Assert
            Assert.NotNull(options);
            Assert.Equal(mockGenerator.Object, options.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOriginalOptions_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<SomeOptions>(sp => new SomeOptions());

            var provider = services.BuildServiceProvider();

            // Act
            var options = MongoServiceCollectionExtensions.GetCollectionOptions(provider, sp => new SomeOptions());

            // Assert
            Assert.NotNull(options);
            Assert.Null(options.EmbeddingGenerator);
        }

        [Fact]
        public void AddKeyedMongoVectorStore_AddsServices_CreatesMongoVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDatabase = new Mock<IMongoDatabase>();
            services.AddSingleton<IMongoDatabase>(mockDatabase.Object);
            var provider = services.BuildServiceProvider();

            // Act
            services.AddKeyedMongoVectorStore("key", "connStr", "dbName");

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var store = serviceProvider.GetService<MongoVectorStore>();
            Assert.NotNull(store);
        }

        [Fact]
        public void AddMongoCollection_AddsServices_CreatesMongoCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDatabase = new Mock<IMongoDatabase>();
            services.AddSingleton<IMongoDatabase>(mockDatabase.Object);
            var provider = services.BuildServiceProvider();

            // Act
            services.AddMongoCollection<SomeRecord>("name");

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var collection = serviceProvider.GetService<MongoCollection<string, SomeRecord>>();
            Assert.NotNull(collection);
        }
    }

    // Dummy classes for options
    public class SomeOptions : MongoVectorStoreOptions
    {
        public IEmbeddingGenerator? EmbeddingGenerator { get; set; }
    }

    public class SomeRecord
    {
        public string Id { get; set; }
    }
}
