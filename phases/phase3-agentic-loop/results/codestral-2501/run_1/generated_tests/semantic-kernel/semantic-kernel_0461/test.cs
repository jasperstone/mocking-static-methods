using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Moq;
using System;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using MongoDB.Driver;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ShouldReturnOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var options = new MongoVectorStoreOptions();

            // Act
            var method = typeof(MongoServiceCollectionExtensions).GetMethod("GetStoreOptions", BindingFlags.NonPublic | BindingFlags.Static);
            var result = method.Invoke(null, new object[] { serviceProviderMock.Object, (Func<IServiceProvider, MongoVectorStoreOptions?>)(_ => options) });

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, ((MongoVectorStoreOptions)result).EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnOptions_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new MongoVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };

            // Act
            var method = typeof(MongoServiceCollectionExtensions).GetMethod("GetStoreOptions", BindingFlags.NonPublic | BindingFlags.Static);
            var result = method.Invoke(null, new object[] { serviceProviderMock.Object, (Func<IServiceProvider, MongoVectorStoreOptions?>)(_ => options) });

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, ((MongoVectorStoreOptions)result).EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var options = new MongoCollectionOptions();

            // Act
            var method = typeof(MongoServiceCollectionExtensions).GetMethod("GetCollectionOptions", BindingFlags.NonPublic | BindingFlags.Static);
            var result = method.Invoke(null, new object[] { serviceProviderMock.Object, (Func<IServiceProvider, MongoCollectionOptions?>)(_ => options) });

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, ((MongoCollectionOptions)result).EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptions_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new MongoCollectionOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };

            // Act
            var method = typeof(MongoServiceCollectionExtensions).GetMethod("GetCollectionOptions", BindingFlags.NonPublic | BindingFlags.Static);
            var result = method.Invoke(null, new object[] { serviceProviderMock.Object, (Func<IServiceProvider, MongoCollectionOptions?>)(_ => options) });

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, ((MongoCollectionOptions)result).EmbeddingGenerator);
        }

        [Fact]
        public void AddKeyedMongoVectorStore_ShouldRegisterServicesCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            var options = new MongoVectorStoreOptions();
            var serviceKey = "testKey";

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IMongoDatabase))).Returns(mongoDatabaseMock.Object);
            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            var result = MongoServiceCollectionExtensions.AddKeyedMongoVectorStore(serviceCollection, serviceKey, options);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var mongoVectorStore = serviceProvider.GetRequiredKeyedService<MongoVectorStore>(serviceKey);
            var vectorStore = serviceProvider.GetRequiredKeyedService<VectorStore>(serviceKey);

            Assert.NotNull(mongoVectorStore);
            Assert.NotNull(vectorStore);
            Assert.Same(mongoVectorStore, vectorStore);
        }

        [Fact]
        public void AddKeyedMongoVectorStore_WithConnectionString_ShouldRegisterServicesCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var options = new MongoVectorStoreOptions();
            var serviceKey = "testKey";
            var connectionString = "mongodb://localhost:27017";
            var databaseName = "testDatabase";

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IMongoDatabase))).Returns(new Mock<IMongoDatabase>().Object);
            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            var result = MongoServiceCollectionExtensions.AddKeyedMongoVectorStore(serviceCollection, serviceKey, connectionString, databaseName, options);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var mongoVectorStore = serviceProvider.GetRequiredKeyedService<MongoVectorStore>(serviceKey);
            var vectorStore = serviceProvider.GetRequiredKeyedService<VectorStore>(serviceKey);

            Assert.NotNull(mongoVectorStore);
            Assert.NotNull(vectorStore);
            Assert.Same(mongoVectorStore, vectorStore);
        }

        [Fact]
        public void AddKeyedMongoCollection_ShouldRegisterServicesCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            var options = new MongoCollectionOptions();
            var serviceKey = "testKey";
            var collectionName = "testCollection";

            serviceProviderMock.Setup(sp => sp.GetService(typeof(IMongoDatabase))).Returns(mongoDatabaseMock.Object);
            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            var result = MongoServiceCollectionExtensions.AddKeyedMongoCollection<TestRecord>(serviceCollection, serviceKey, collectionName, options);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var mongoCollection = serviceProvider.GetRequiredKeyedService<MongoCollection<string, TestRecord>>(serviceKey);
            var vectorStoreCollection = serviceProvider.GetRequiredKeyedService<VectorStoreCollection<string, TestRecord>>(serviceKey);
            var vectorSearchable = serviceProvider.GetRequiredKeyedService<IVectorSearchable<TestRecord>>(serviceKey);
            var keywordHybridSearchable = serviceProvider.GetRequiredKeyedService<IKeywordHybridSearchable<TestRecord>>(serviceKey);

            Assert.NotNull(mongoCollection);
            Assert.NotNull(vectorStoreCollection);
            Assert.NotNull(vectorSearchable);
            Assert.NotNull(keywordHybridSearchable);
            Assert.Same(mongoCollection, vectorStoreCollection);
            Assert.Same(mongoCollection, vectorSearchable);
            Assert.Same(mongoCollection, keywordHybridSearchable);
        }

        private class TestRecord
        {
            // Test record properties
        }
    }
}
