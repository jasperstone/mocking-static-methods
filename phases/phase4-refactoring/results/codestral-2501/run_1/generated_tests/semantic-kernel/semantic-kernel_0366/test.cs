using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Moq;
using System;

namespace SemanticKernel.Core.Tests.Data.TextSearch
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_ShouldRegisterVectorStoreTextSearch()
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
            services.AddVectorStoreTextSearch<string>(vectorSearchServiceId, textEmbeddingGenerationServiceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var vectorStoreTextSearch = serviceProvider.GetService<VectorStoreTextSearch<string>>();
            Assert.NotNull(vectorStoreTextSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_ShouldThrowInvalidOperationException_WhenVectorSearchableNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var vectorSearchServiceId = "vectorSearchServiceId";
            var textEmbeddingGenerationServiceId = "textEmbeddingGenerationServiceId";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => services.AddVectorStoreTextSearch<string>(vectorSearchServiceId, textEmbeddingGenerationServiceId));
            Assert.Equal($"No IVectorizedSearch<string> for service id {vectorSearchServiceId} registered.", exception.Message);
        }

        [Fact]
        public void AddVectorStoreTextSearch_ShouldThrowInvalidOperationException_WhenTextEmbeddingGenerationServiceNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var vectorSearchServiceId = "vectorSearchServiceId";
            var textEmbeddingGenerationServiceId = "textEmbeddingGenerationServiceId";
            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();

            services.AddSingleton(mockVectorSearchable.Object);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => services.AddVectorStoreTextSearch<string>(vectorSearchServiceId, textEmbeddingGenerationServiceId));
            Assert.Equal($"No ITextEmbeddingGenerationService for service id {textEmbeddingGenerationServiceId} registered.", exception.Message);
        }
    }
}
