using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new MongoVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };

            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsProvidedByServiceProvider()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var originalOptions = new MongoVectorStoreOptions();

            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => originalOptions);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(originalOptions, result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenNoOptionsAndNoEmbeddingGenerator()
        {
            // Arrange
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptions_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new MongoCollectionOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };

            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var result = InvokeGetCollectionOptions(spMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsProvidedByServiceProvider()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var originalOptions = new MongoCollectionOptions();

            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var result = InvokeGetCollectionOptions(spMock.Object, _ => originalOptions);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(originalOptions, result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNull_WhenNoOptionsAndNoEmbeddingGenerator()
        {
            // Arrange
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            // Act
            var result = InvokeGetCollectionOptions(spMock.Object, null);

            // Assert
            Assert.Null(result);
        }

        // Helper methods to invoke private static methods via reflection
        private static MongoVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, MongoVectorStoreOptions?>? optionsProvider)
        {
            var method = typeof(MongoServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (MongoVectorStoreOptions?)method!.Invoke(null, new object?[] { sp, optionsProvider });
        }

        private static MongoCollectionOptions? InvokeGetCollectionOptions(IServiceProvider sp, Func<IServiceProvider, MongoCollectionOptions?>? optionsProvider)
        {
            var method = typeof(MongoServiceCollectionExtensions).GetMethod("GetCollectionOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (MongoCollectionOptions?)method!.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
