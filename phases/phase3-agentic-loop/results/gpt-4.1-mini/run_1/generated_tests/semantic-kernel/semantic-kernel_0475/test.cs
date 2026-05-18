using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Qdrant.Client;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        // We use Moq to mock IEmbeddingGenerator because the interface has members that are not trivial to implement.

        private class DummyRecord { }

        [Fact]
        public void GetCollectionOptions_UsesEmbeddingGeneratorFromServiceProvider_WhenOptionsProviderDoesNotProvide()
        {
            // Arrange
            var services = new ServiceCollection();

            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            services.AddSingleton(embeddingGeneratorMock.Object);

            // Add dummy QdrantClient to satisfy dependencies
            services.AddSingleton<QdrantClient>(new QdrantClient("localhost"));

            var provider = services.BuildServiceProvider();

            // Act
            var options = InvokeGetCollectionOptions(provider, sp => new QdrantCollectionOptions());

            // Assert
            Assert.NotNull(options);
            Assert.NotNull(options.EmbeddingGenerator);
            Assert.Same(embeddingGeneratorMock.Object, options.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsProviderOptions_WhenEmbeddingGeneratorIsProvided()
        {
            // Arrange
            var services = new ServiceCollection();

            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var optionsWithEmbedding = new QdrantCollectionOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };

            services.AddSingleton<QdrantClient>(new QdrantClient("localhost"));

            var provider = services.BuildServiceProvider();

            // Act
            var options = InvokeGetCollectionOptions(provider, sp => optionsWithEmbedding);

            // Assert
            Assert.NotNull(options);
            Assert.Same(embeddingGeneratorMock.Object, options.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNull_WhenNoOptionsProviderAndNoEmbeddingGenerator()
        {
            // Arrange
            var services = new ServiceCollection();

            services.AddSingleton<QdrantClient>(new QdrantClient("localhost"));

            var provider = services.BuildServiceProvider();

            // Act
            var options = InvokeGetCollectionOptions(provider, null);

            // Assert
            Assert.Null(options);
        }

        private static QdrantCollectionOptions? InvokeGetCollectionOptions(IServiceProvider sp, Func<IServiceProvider, QdrantCollectionOptions?>? optionsProvider)
        {
            var type = typeof(QdrantServiceCollectionExtensions);
            var method = type.GetMethod("GetCollectionOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);

            var result = method.Invoke(null, new object?[] { sp, optionsProvider });
            return (QdrantCollectionOptions?)result;
        }
    }
}
