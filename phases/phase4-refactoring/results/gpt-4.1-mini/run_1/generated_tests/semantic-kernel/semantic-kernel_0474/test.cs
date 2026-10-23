using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsHasEmbeddingGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var embeddingGenerator = embeddingGeneratorMock.Object;
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGenerator };
            services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
            var sp = services.BuildServiceProvider();

            // Act
            var result = InvokeGetStoreOptions(sp, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenNoEmbeddingGeneratorInOptionsAndNoService()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantVectorStoreOptions();
            var sp = services.BuildServiceProvider();

            // Act
            var result = InvokeGetStoreOptions(sp, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenNoEmbeddingGeneratorInOptionsButServiceExists()
        {
            // Arrange
            var services = new ServiceCollection();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var embeddingGenerator = embeddingGeneratorMock.Object;
            services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
            var sp = services.BuildServiceProvider();

            var options = new QdrantVectorStoreOptions();

            // Act
            var result = InvokeGetStoreOptions(sp, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenOptionsProviderReturnsNullAndNoService()
        {
            // Arrange
            var services = new ServiceCollection();
            var sp = services.BuildServiceProvider();

            // Act
            var result = InvokeGetStoreOptions(sp, _ => null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderReturnsNullButServiceExists()
        {
            // Arrange
            var services = new ServiceCollection();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var embeddingGenerator = embeddingGeneratorMock.Object;
            services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
            var sp = services.BuildServiceProvider();

            // Act
            var result = InvokeGetStoreOptions(sp, _ => null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        private static QdrantVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, QdrantVectorStoreOptions?>? optionsProvider)
        {
            // Use reflection to invoke the private static method GetStoreOptions
            var type = typeof(QdrantServiceCollectionExtensions);
            var method = type.GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (QdrantVectorStoreOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
