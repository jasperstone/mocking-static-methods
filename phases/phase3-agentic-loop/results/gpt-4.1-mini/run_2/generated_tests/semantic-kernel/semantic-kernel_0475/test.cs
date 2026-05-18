using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        // We test the private static method GetCollectionOptions indirectly by invoking AddQdrantCollection
        // but since it's private, we will test GetStoreOptions instead which has the same logic and is also private.
        // To test the call to IServiceProvider.GetService<IEmbeddingGenerator>(), we will mock IServiceProvider.

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };

            var spMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            // The optionsProvider returns options with EmbeddingGenerator set, so GetService should not be called.
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
            spMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Never);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenEmbeddingGeneratorIsNull()
        {
            // Arrange
            var options = new QdrantVectorStoreOptions(); // EmbeddingGenerator is null

            var spMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
            spMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorProvidedByServiceProvider()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions(); // EmbeddingGenerator is null

            var spMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
            spMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenOptionsProviderIsNullAndNoEmbeddingGenerator()
        {
            // Arrange
            var spMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, null);

            // Assert
            Assert.Null(result);
            spMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
        }

        // Helper method to invoke the private static GetStoreOptions method via reflection
        private static QdrantVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, QdrantVectorStoreOptions?>? optionsProvider)
        {
            var type = typeof(QdrantServiceCollectionExtensions);
            var method = type.GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            return (QdrantVectorStoreOptions?)method.Invoke(null, new object?[] { sp, optionsProvider! });
        }
    }
}
