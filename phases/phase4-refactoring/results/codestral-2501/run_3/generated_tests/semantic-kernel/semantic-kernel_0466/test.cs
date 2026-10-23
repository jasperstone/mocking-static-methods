using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Moq;
using Xunit;
using Microsoft.Extensions.AI;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_OptionsProviderNull_ReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = null;

            // Act
            var result = CallGetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_OptionsProviderReturnsOptionsWithEmbeddingGenerator_ReturnsOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var options = new PostgresVectorStoreOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };
            Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = sp => options;

            // Act
            var result = CallGetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_OptionsProviderReturnsOptionsWithoutEmbeddingGeneratorAndServiceProviderReturnsEmbeddingGenerator_ReturnsNewOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);
            var options = new PostgresVectorStoreOptions();
            Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = sp => options;

            // Act
            var result = CallGetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotSame(options, result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_OptionsProviderReturnsOptionsWithoutEmbeddingGeneratorAndServiceProviderReturnsNull_ReturnsOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);
            var options = new PostgresVectorStoreOptions();
            Func<IServiceProvider, PostgresVectorStoreOptions?> optionsProvider = sp => options;

            // Act
            var result = CallGetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
        }

        private PostgresVectorStoreOptions? CallGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, PostgresVectorStoreOptions?>? optionsProvider)
        {
            // This is a workaround to call the private method GetStoreOptions
            var method = typeof(PostgresServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (PostgresVectorStoreOptions?)method.Invoke(null, new object[] { sp, optionsProvider });
        }
    }
}
