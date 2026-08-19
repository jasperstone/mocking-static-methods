using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddingGenerator_WhenOptionsProviderReturnsOptionsWithoutEmbeddingGenerator_AndServiceProviderHasEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var embeddingGenerator = embeddingGeneratorMock.Object;

            var optionsWithoutEmbedding = new PostgresVectorStoreOptions();

            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGenerator);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => optionsWithoutEmbedding);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(optionsWithoutEmbedding, result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsDirectly_WhenOptionsProviderReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var embeddingGenerator = embeddingGeneratorMock.Object;

            var optionsWithEmbedding = new PostgresVectorStoreOptions { EmbeddingGenerator = embeddingGenerator };

            var spMock = new Mock<IServiceProvider>();

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => optionsWithEmbedding);

            // Assert
            Assert.Same(optionsWithEmbedding, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenOptionsProviderReturnsNull_AndServiceProviderHasNoEmbeddingGenerator()
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
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderReturnsNull_AndServiceProviderHasEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var embeddingGenerator = embeddingGeneratorMock.Object;

            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGenerator);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        private static PostgresVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, PostgresVectorStoreOptions?>? optionsProvider)
        {
            // Use reflection to invoke the private static method GetStoreOptions
            var method = typeof(PostgresServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            return (PostgresVectorStoreOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
