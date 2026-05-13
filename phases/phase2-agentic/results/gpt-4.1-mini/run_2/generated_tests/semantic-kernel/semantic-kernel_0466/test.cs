using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_UsesEmbeddingGeneratorFromServiceProvider_WhenOptionsProviderReturnsNull()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(embeddingGeneratorMock.Object);

            // Act
            var options = InvokeGetStoreOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.NotNull(options);
            Assert.Same(embeddingGeneratorMock.Object, options.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsDirectly_WhenOptionsProviderReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var optionsFromProvider = new PostgresVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            var options = InvokeGetStoreOptions(serviceProviderMock.Object, sp => optionsFromProvider);

            // Assert
            Assert.Same(optionsFromProvider, options);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenOptionsProviderReturnsNullAndNoEmbeddingGeneratorInServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(null);

            // Act
            var options = InvokeGetStoreOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.Null(options);
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
