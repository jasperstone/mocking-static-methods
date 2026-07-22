using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Microsoft.Extensions.AI;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_WithEmbeddingGeneratorFromServiceProvider_ReturnsNewOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(embeddingGeneratorMock.Object);

            MongoVectorStoreOptions? options = new MongoVectorStoreOptions();

            // Act
            var result = InvokeGetStoreOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_WithOptionsHavingEmbeddingGenerator_ReturnsSameOptions()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new MongoVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            var result = InvokeGetStoreOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_WithNoEmbeddingGeneratorInServiceProvider_ReturnsOriginalOptions()
        {
            // Arrange
            MongoVectorStoreOptions? options = new MongoVectorStoreOptions();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(null);

            // Act
            var result = InvokeGetStoreOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_WithNullOptionsAndNoEmbeddingGenerator_ReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(null);

            // Act
            var result = InvokeGetStoreOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.Null(result);
        }

        private static MongoVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, MongoVectorStoreOptions?>? optionsProvider)
        {
            // Use reflection to invoke the private static method GetStoreOptions
            var method = typeof(MongoServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            return (MongoVectorStoreOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
