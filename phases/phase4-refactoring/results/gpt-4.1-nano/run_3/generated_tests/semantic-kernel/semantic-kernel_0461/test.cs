using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using MongoDB.Driver;
using System;

namespace VectorData.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddedGenerator_WhenServiceProvidesGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new MongoVectorStoreOptions();

            mockProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>())
                        .Returns(mockGenerator.Object);

            // Use reflection to invoke the private method
            var method = typeof(MongoServiceCollectionExtensions)
                .GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { mockProvider.Object, new Func<IServiceProvider, MongoVectorStoreOptions?>(_ => options) });

            // Assert
            Assert.NotNull(result);
            var resultOptions = result as MongoVectorStoreOptions;
            Assert.NotNull(resultOptions);
            Assert.Equal(mockGenerator.Object, resultOptions.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenServiceDoesNotProvideGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>())
                        .Returns((IEmbeddingGenerator)null);
            var options = new MongoVectorStoreOptions();

            // Use reflection to invoke the private method
            var method = typeof(MongoServiceCollectionExtensions)
                .GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { mockProvider.Object, new Func<IServiceProvider, MongoVectorStoreOptions?>(_ => options) });

            // Assert
            Assert.NotNull(result);
            var resultOptions = result as MongoVectorStoreOptions;
            Assert.NotNull(resultOptions);
            Assert.Null(resultOptions.EmbeddingGenerator);
            Assert.Equal(options, resultOptions);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithEmbeddedGenerator_WhenServiceProvidesGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new MongoCollectionOptions();

            mockProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>())
                        .Returns(mockGenerator.Object);

            // Use reflection to invoke the private method
            var method = typeof(MongoServiceCollectionExtensions)
                .GetMethod("GetCollectionOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { mockProvider.Object, new Func<IServiceProvider, MongoCollectionOptions?>(_ => options) });

            // Assert
            Assert.NotNull(result);
            var resultOptions = result as MongoCollectionOptions;
            Assert.NotNull(resultOptions);
            Assert.Equal(mockGenerator.Object, resultOptions.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOriginalOptions_WhenServiceDoesNotProvideGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>())
                        .Returns((IEmbeddingGenerator)null);
            var options = new MongoCollectionOptions();

            // Use reflection to invoke the private method
            var method = typeof(MongoServiceCollectionExtensions)
                .GetMethod("GetCollectionOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Act
            var result = method.Invoke(null, new object[] { mockProvider.Object, new Func<IServiceProvider, MongoCollectionOptions?>(_ => options) });

            // Assert
            Assert.NotNull(result);
            var resultOptions = result as MongoCollectionOptions;
            Assert.NotNull(resultOptions);
            Assert.Null(resultOptions.EmbeddingGenerator);
            Assert.Equal(options, resultOptions);
        }

        [Fact]
        public void AddKeyedMongoVectorStore_RegistersServices_CreatesMongoVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = new ServiceCollection()
                .AddSingleton<IMongoDatabase>(new Mock<IMongoDatabase>().Object)
                .BuildServiceProvider();

            // Act
            var result = MongoServiceCollectionExtensions.AddKeyedMongoVectorStore(services, "testKey", "connStr", "dbName");

            // Assert
            Assert.Contains(result, d => d.ServiceType == typeof(MongoVectorStore));
            Assert.Contains(result, d => d.ServiceType == typeof(VectorStore));
        }
    }
}
