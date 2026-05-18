using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using System;

namespace SemanticKernel.Core.Tests.Data.TextSearch
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_WithValidServices_ShouldRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();
            var vectorSearchServiceId = "vectorSearchServiceId";
            var textEmbeddingGenerationServiceId = "textEmbeddingGenerationServiceId";
            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();
            var mockTextEmbeddingGenerationService = new Mock<ITextEmbeddingGenerationService>();

            services.AddSingleton(mockVectorSearchable.Object);
            services.AddSingleton(mockTextEmbeddingGenerationService.Object);

            // Act
            services.AddVectorStoreTextSearch<string>(
                vectorSearchServiceId,
                textEmbeddingGenerationServiceId);

            var serviceProvider = services.BuildServiceProvider();
            var vectorStoreTextSearch = serviceProvider.GetService<VectorStoreTextSearch<string>>();

            // Assert
            Assert.NotNull(vectorStoreTextSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithInvalidVectorSearchServiceId_ShouldThrowException()
        {
            // Arrange
            var services = new ServiceCollection();
            var vectorSearchServiceId = "invalidVectorSearchServiceId";
            var textEmbeddingGenerationServiceId = "textEmbeddingGenerationServiceId";
            var mockTextEmbeddingGenerationService = new Mock<ITextEmbeddingGenerationService>();

            services.AddSingleton(mockTextEmbeddingGenerationService.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<string>(
                    vectorSearchServiceId,
                    textEmbeddingGenerationServiceId));
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithInvalidTextEmbeddingGenerationServiceId_ShouldThrowException()
        {
            // Arrange
            var services = new ServiceCollection();
            var vectorSearchServiceId = "vectorSearchServiceId";
            var textEmbeddingGenerationServiceId = "invalidTextEmbeddingGenerationServiceId";
            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();

            services.AddSingleton(mockVectorSearchable.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<string>(
                    vectorSearchServiceId,
                    textEmbeddingGenerationServiceId));
        }
    }
}
