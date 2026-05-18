using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.Extensions.VectorData;
using MongoDB.Driver;

namespace VectorData.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddingGenerator_WhenServiceProvidesGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new MongoVectorStoreOptions();

            mockProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>())
                        .Returns(mockGenerator.Object);

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(mockProvider.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var options = new MongoVectorStoreOptions();

            mockProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>())
                        .Returns((IEmbeddingGenerator?)null);

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(mockProvider.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
            Assert.Null(result.EmbeddingGenerator);
        }

        [Fact]
        public void AddKeyedMongoVectorStore_AddsServices_CreatesMongoVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDatabase = new Mock<IMongoDatabase>();
            services.AddSingleton(mockDatabase.Object);
            var provider = services.BuildServiceProvider();

            // Act
            services.AddKeyedMongoVectorStore("key", "connStr", "dbName");
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var store = serviceProvider.GetService<VectorStore>();
            Assert.NotNull(store);
        }

        [Fact]
        public void AddKeyedMongoVectorStore_UsesProvidedOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockDatabase = new Mock<IMongoDatabase>();
            services.AddSingleton(mockDatabase.Object);
            var options = new MongoVectorStoreOptions { /* set properties if needed */ };

            // Act
            services.AddKeyedMongoVectorStore("key", "connStr", "dbName", options);
            var provider = services.BuildServiceProvider();

            // Assert
            var store = provider.GetService<VectorStore>();
            Assert.NotNull(store);
        }
    }
}
