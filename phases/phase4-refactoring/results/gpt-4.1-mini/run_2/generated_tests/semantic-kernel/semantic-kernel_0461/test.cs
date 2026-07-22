using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.MongoDB;
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
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(embeddingGeneratorMock.Object);

            MongoVectorStoreOptions? options = new MongoVectorStoreOptions();

            // Act
            var result = InvokeGetStoreOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.EmbeddingGenerator);
            Assert.NotSame(options, result);
            Assert.Equal(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_WithNullEmbeddingGeneratorFromServiceProvider_ReturnsOriginalOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(null);

            MongoVectorStoreOptions? options = new MongoVectorStoreOptions();

            // Act
            var result = InvokeGetStoreOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_WithNullOptionsAndNullEmbeddingGenerator_ReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(null);

            // Act
            var result = InvokeGetStoreOptions(serviceProviderMock.Object, _ => null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_WithOptionsHavingEmbeddingGenerator_ReturnsSameOptions()
        {
            // Arrange
            var options = new MongoVectorStoreOptions
            {
                EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object
            };
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            var result = InvokeGetStoreOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
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
