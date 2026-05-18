using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Xunit;
using Microsoft.Extensions.AI;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetCollectionOptions_ShouldCallGetService_WhenEmbeddingGeneratorIsNull()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(mockEmbeddingGenerator.Object);

            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => null);

            // Act
            var result = InvokeGetCollectionOptions(mockServiceProvider.Object, optionsProvider);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
            Assert.NotNull(result);
            Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldNotCallGetService_WhenEmbeddingGeneratorIsProvided()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();

            var options = new QdrantCollectionOptions
            {
                EmbeddingGenerator = mockEmbeddingGenerator.Object
            };

            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => options);

            // Act
            var result = InvokeGetCollectionOptions(mockServiceProvider.Object, optionsProvider);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Never);
            Assert.NotNull(result);
            Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
        }

        private QdrantCollectionOptions? InvokeGetCollectionOptions(IServiceProvider sp, Func<IServiceProvider, QdrantCollectionOptions?>? optionsProvider)
        {
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetCollectionOptions", BindingFlags.NonPublic | BindingFlags.Static);
            return (QdrantCollectionOptions?)method?.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
