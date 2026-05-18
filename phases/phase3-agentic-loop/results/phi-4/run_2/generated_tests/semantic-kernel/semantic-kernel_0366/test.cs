using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;

namespace Microsoft.SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_AllServicesProvided_ShouldReturnServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = new Mock<IServiceProvider>();
            var stringMapper = new Mock<ITextSearchStringMapper>();
            var resultMapper = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();
            var vectorSearch = new Mock<IVectorSearchable<object>>();
            var generationService = new Mock<ITextEmbeddingGenerationService>();

            serviceProvider
                .Setup(sp => sp.GetService<ITextSearchStringMapper>())
                .Returns(stringMapper.Object);
            serviceProvider
                .Setup(sp => sp.GetService<ITextSearchResultMapper>())
                .Returns(resultMapper.Object);
            serviceProvider
                .Setup(sp => sp.GetService<VectorStoreTextSearchOptions>())
                .Returns(options);
            serviceProvider
                .Setup(sp => sp.GetKeyedService<IVectorSearchable<object>>("vectorSearchId"))
                .Returns(vectorSearch.Object);
            serviceProvider
                .Setup(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>("generationServiceId"))
                .Returns(generationService.Object);

            // Act
            var result = services.AddVectorStoreTextSearch<object>(
                "vectorSearchId",
                "generationServiceId",
                serviceProvider.Object);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddVectorStoreTextSearch_SomeServicesNull_ShouldFetchFromServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = new Mock<IServiceProvider>();
            var stringMapper = new Mock<ITextSearchStringMapper>();
            var resultMapper = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();
            var vectorSearch = new Mock<IVectorSearchable<object>>();
            var generationService = new Mock<ITextEmbeddingGenerationService>();

            serviceProvider
                .Setup(sp => sp.GetService<ITextSearchStringMapper>())
                .Returns(stringMapper.Object);
            serviceProvider
                .Setup(sp => sp.GetService<ITextSearchResultMapper>())
                .Returns(resultMapper.Object);
            serviceProvider
                .Setup(sp => sp.GetService<VectorStoreTextSearchOptions>())
                .Returns(options);
            serviceProvider
                .Setup(sp => sp.GetKeyedService<IVectorSearchable<object>>("vectorSearchId"))
                .Returns(vectorSearch.Object);
            serviceProvider
                .Setup(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>("generationServiceId"))
                .Returns(generationService.Object);

            // Act
            var result = services.AddVectorStoreTextSearch<object>(
                "vectorSearchId",
                "generationServiceId",
                null,
                null,
                null,
                serviceProvider.Object);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddVectorStoreTextSearch_VectorSearchNotRegistered_ShouldThrowException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = new Mock<IServiceProvider>();

            serviceProvider
                .Setup(sp => sp.GetKeyedService<IVectorSearchable<object>>("vectorSearchId"))
                .Returns((IVectorSearchable<object>)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<object>(
                    "vectorSearchId",
                    "generationServiceId",
                    serviceProvider.Object));
        }

        [Fact]
        public void AddVectorStoreTextSearch_GenerationServiceNotRegistered_ShouldThrowException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = new Mock<IServiceProvider>();
            var vectorSearch = new Mock<IVectorSearchable<object>>();

            serviceProvider
                .Setup(sp => sp.GetKeyedService<IVectorSearchable<object>>("vectorSearchId"))
                .Returns(vectorSearch.Object);
            serviceProvider
                .Setup(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>("generationServiceId"))
                .Returns((ITextEmbeddingGenerationService)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<object>(
                    "vectorSearchId",
                    "generationServiceId",
                    serviceProvider.Object));
        }
    }
}
