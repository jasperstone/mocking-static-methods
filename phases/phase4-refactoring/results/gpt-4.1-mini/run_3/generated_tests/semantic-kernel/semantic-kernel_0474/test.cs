using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.AI;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        private class TestServiceProvider : IServiceProvider
        {
            private readonly object _serviceToReturn;

            public TestServiceProvider(object serviceToReturn)
            {
                _serviceToReturn = serviceToReturn;
            }

            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof(IEmbeddingGenerator))
                {
                    return _serviceToReturn;
                }
                return null;
            }
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsIfEmbeddingGeneratorIsSet()
        {
            // Arrange
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };
            var sp = new Mock<IServiceProvider>(MockBehavior.Strict);

            // Act
            var result = InvokeGetStoreOptions(sp.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsIfNoEmbeddingGeneratorInServiceProvider()
        {
            // Arrange
            var options = new QdrantVectorStoreOptions();
            var sp = new TestServiceProvider(null);

            // Act
            var result = InvokeGetStoreOptions(sp, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGeneratorIfNotSetInOptions()
        {
            // Arrange
            var embeddingGenerator = new Mock<IEmbeddingGenerator>().Object;
            var sp = new TestServiceProvider(embeddingGenerator);
            var options = new QdrantVectorStoreOptions();

            // Act
            var result = InvokeGetStoreOptions(sp, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNullIfOptionsProviderReturnsNullAndNoEmbeddingGenerator()
        {
            // Arrange
            var sp = new TestServiceProvider(null);

            // Act
            var result = InvokeGetStoreOptions(sp, null);

            // Assert
            Assert.Null(result);
        }

        private static QdrantVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, QdrantVectorStoreOptions?>? optionsProvider)
        {
            // Use reflection to invoke the private static method GetStoreOptions
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            return (QdrantVectorStoreOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
