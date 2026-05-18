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
            var mockProvider = new Mock<IServiceProvider>();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new MongoVectorStoreOptions();

            mockProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>())
                        .Returns(mockGenerator.Object);

            // Use reflection to invoke the private method
            var methodInfo = typeof(MongoServiceCollectionExtensions)
                .GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var result = methodInfo.Invoke(null, new object[] { mockProvider.Object, new Func<IServiceProvider, MongoVectorStoreOptions?>(_ => options) });

            // Assert
            Assert.NotNull(result);
            var optionsResult = result as MongoVectorStoreOptions;
            Assert.NotNull(optionsResult);
            Assert.Equal(mockGenerator.Object, optionsResult.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenServiceProvidesNoGenerator()
        {
            // Arrange
            var mockProvider = new Mock<IServiceProvider>();
            var options = new MongoVectorStoreOptions();

            mockProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>())
                        .Returns((IEmbeddingGenerator?)null);

            var methodInfo = typeof(MongoServiceCollectionExtensions)
                .GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var result = methodInfo.Invoke(null, new object[] { mockProvider.Object, new Func<IServiceProvider, MongoVectorStoreOptions?>(_ => options) });

            // Assert
            Assert.NotNull(result);
            var optionsResult = result as MongoVectorStoreOptions;
            Assert.NotNull(optionsResult);
            Assert.Equal(options, optionsResult);
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
            services.AddKeyedMongoVectorStore("testKey", null, null, ServiceLifetime.Singleton);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var store = serviceProvider.GetService<VectorStore>();
            Assert.NotNull(store);
        }
    }
}
