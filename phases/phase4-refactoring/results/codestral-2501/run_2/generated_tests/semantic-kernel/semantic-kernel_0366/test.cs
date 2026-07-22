using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests.Data.TextSearch
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_WithValidServices_ShouldRegisterService()
        {
            // Arrange
            var services = new ServiceCollection();
            var vectorSearchableServiceId = "vectorSearchableServiceId";
            var textEmbeddingGenerationServiceId = "textEmbeddingGenerationServiceId";

            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();
            var mockTextEmbeddingGenerationService = new Mock<ITextEmbeddingGenerationService>();

            services.AddSingleton(mockVectorSearchable.Object);
            services.AddSingleton(mockTextEmbeddingGenerationService.Object);

            // Act
            services.AddVectorStoreTextSearch<string>(
                vectorSearchableServiceId,
                textEmbeddingGenerationServiceId);

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var vectorStoreTextSearch = serviceProvider.GetService<VectorStoreTextSearch<string>>();
            Assert.NotNull(vectorStoreTextSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithInvalidVectorSearchableServiceId_ShouldThrowException()
        {
            // Arrange
            var services = new ServiceCollection();
            var vectorSearchableServiceId = "invalidVectorSearchableServiceId";
            var textEmbeddingGenerationServiceId = "textEmbeddingGenerationServiceId";

            var mockTextEmbeddingGenerationService = new Mock<ITextEmbeddingGenerationService>();
            services.AddSingleton(mockTextEmbeddingGenerationService.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<string>(
                    vectorSearchableServiceId,
                    textEmbeddingGenerationServiceId));
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithInvalidTextEmbeddingGenerationServiceId_ShouldThrowException()
        {
            // Arrange
            var services = new ServiceCollection();
            var vectorSearchableServiceId = "vectorSearchableServiceId";
            var textEmbeddingGenerationServiceId = "invalidTextEmbeddingGenerationServiceId";

            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();
            services.AddSingleton(mockVectorSearchable.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<string>(
                    vectorSearchableServiceId,
                    textEmbeddingGenerationServiceId));
        }
    }
}
