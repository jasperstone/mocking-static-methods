using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsHasEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new MongoVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };
            var spMock = new Mock<IServiceProvider>();

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsHasNoEmbeddingGeneratorAndServiceProviderHasEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new MongoVectorStoreOptions { EmbeddingGenerator = null };
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsIsNullAndServiceProviderHasNoEmbeddingGenerator()
        {
            // Arrange
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsHasNoEmbeddingGeneratorAndServiceProviderHasNoEmbeddingGenerator()
        {
            // Arrange
            var options = new MongoVectorStoreOptions { EmbeddingGenerator = null };
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        // Helper method to invoke the private static GetStoreOptions method via reflection
        private static MongoVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, MongoVectorStoreOptions?>? optionsProvider)
        {
            var method = typeof(MongoServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            return (MongoVectorStoreOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
