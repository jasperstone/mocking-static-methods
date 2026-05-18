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

            // Act
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

            // Act
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
        public void GetStoreOptions_ReturnsNewOptionsWithGenerator_WhenGeneratorIsAvailable()
        {
            // Arrange
            var mockProvider = new Mock<IServiceProvider>();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new MongoVectorStoreOptions();

            mockProvider.Setup(sp => sp.GetService<IEmbeddingGenerator>())
                        .Returns(mockGenerator.Object);

            // Act
            var methodInfo = typeof(MongoServiceCollectionExtensions)
                .GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var result = methodInfo.Invoke(null, new object[] { mockProvider.Object, new Func<IServiceProvider, MongoVectorStoreOptions?>(_ => options) });

            // Assert
            Assert.NotNull(result);
            var optionsResult = result as MongoVectorStoreOptions;
            Assert.NotNull(optionsResult);
            Assert.NotSame(options, optionsResult);
            Assert.Equal(mockGenerator.Object, optionsResult.EmbeddingGenerator);
        }
    }
}
