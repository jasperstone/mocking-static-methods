using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.MongoDB;
using Moq;
using System;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using MongoDB.Driver;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class MongoServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ShouldReturnOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var options = new MongoVectorStoreOptions();

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnOriginalOptionsIfEmbeddingGeneratorIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            var options = new MongoVectorStoreOptions();

            // Act
            var result = MongoServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var options = new MongoCollectionOptions();

            // Act
            var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOriginalOptionsIfEmbeddingGeneratorIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            var options = new MongoCollectionOptions();

            // Act
            var result = MongoServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void CreateClientSettings_ShouldReturnCorrectSettings()
        {
            // Arrange
            var connectionString = "mongodb://localhost:27017";

            // Act
            var result = MongoServiceCollectionExtensions.CreateClientSettings(connectionString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Microsoft.Extensions.VectorData", result.LibraryInfo.ProductName);
            Assert.Equal("Microsoft.Extensions.VectorData", result.ApplicationName);
        }
    }
}
