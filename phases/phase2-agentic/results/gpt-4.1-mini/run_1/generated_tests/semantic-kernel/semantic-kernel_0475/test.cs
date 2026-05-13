using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        // We test the private static method GetCollectionOptions via reflection
        // because it is private and static.
        // This method calls IServiceProvider.GetService<IEmbeddingGenerator>() on line 262.

        private static QdrantCollectionOptions? InvokeGetCollectionOptions(IServiceProvider sp, Func<IServiceProvider, QdrantCollectionOptions?>? optionsProvider)
        {
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetCollectionOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            return (QdrantCollectionOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsDirectly_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };
            var spMock = new Mock<IServiceProvider>(MockBehavior.Strict);

            // optionsProvider returns options with non-null EmbeddingGenerator
            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

            // Act
            var result = InvokeGetCollectionOptions(spMock.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
            spMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptions_WhenEmbeddingGeneratorIsNullAndServiceProviderReturnsNull()
        {
            // Arrange
            var options = new QdrantCollectionOptions(); // EmbeddingGenerator is null
            var spMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

            // Act
            var result = InvokeGetCollectionOptions(spMock.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
            spMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
            spMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsNullInOptionsButProvidedByServiceProvider()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new QdrantCollectionOptions(); // EmbeddingGenerator is null
            var spMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

            // Act
            var result = InvokeGetCollectionOptions(spMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
            spMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
            spMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNull_WhenOptionsProviderIsNullAndServiceProviderReturnsNull()
        {
            // Arrange
            var spMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            // Act
            var result = InvokeGetCollectionOptions(spMock.Object, null);

            // Assert
            Assert.Null(result);
            spMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
            spMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderIsNullButServiceProviderReturnsEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var spMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var result = InvokeGetCollectionOptions(spMock.Object, null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
            spMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
            spMock.VerifyNoOtherCalls();
        }
    }
}
