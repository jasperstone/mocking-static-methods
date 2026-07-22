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
        public void AddVectorStoreTextSearch_WithValidServices_RegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            var vectorSearchServiceId = "vectorSearchServiceId";
            var textEmbeddingGenerationServiceId = "textEmbeddingGenerationServiceId";

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();
            var mockTextEmbeddingGenerationService = new Mock<ITextEmbeddingGenerationService>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(mockStringMapper.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(mockResultMapper.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(mockOptions.Object);
            mockServiceProvider.Setup(sp => sp.GetKeyedService(typeof(IVectorSearchable<string>), vectorSearchServiceId)).Returns(mockVectorSearchable.Object);
            mockServiceProvider.Setup(sp => sp.GetKeyedService(typeof(ITextEmbeddingGenerationService), textEmbeddingGenerationServiceId)).Returns(mockTextEmbeddingGenerationService.Object);

            services.AddSingleton<IServiceProvider>(mockServiceProvider.Object);

            // Act
            services.AddVectorStoreTextSearch<string>(
                vectorSearchServiceId,
                textEmbeddingGenerationServiceId,
                mockStringMapper.Object,
                mockResultMapper.Object,
                mockOptions.Object);

            var serviceProvider = services.BuildServiceProvider();
            var vectorStoreTextSearch = serviceProvider.GetService<VectorStoreTextSearch<string>>();

            // Assert
            Assert.NotNull(vectorStoreTextSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithMissingVectorSearchable_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var vectorSearchServiceId = "vectorSearchServiceId";
            var textEmbeddingGenerationServiceId = "textEmbeddingGenerationServiceId";

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockTextEmbeddingGenerationService = new Mock<ITextEmbeddingGenerationService>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(mockStringMapper.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(mockResultMapper.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(mockOptions.Object);
            mockServiceProvider.Setup(sp => sp.GetKeyedService(typeof(ITextEmbeddingGenerationService), textEmbeddingGenerationServiceId)).Returns(mockTextEmbeddingGenerationService.Object);

            services.AddSingleton<IServiceProvider>(mockServiceProvider.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<string>(
                    vectorSearchServiceId,
                    textEmbeddingGenerationServiceId,
                    mockStringMapper.Object,
                    mockResultMapper.Object,
                    mockOptions.Object));
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithMissingTextEmbeddingGenerationService_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var vectorSearchServiceId = "vectorSearchServiceId";
            var textEmbeddingGenerationServiceId = "textEmbeddingGenerationServiceId";

            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(mockStringMapper.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(mockResultMapper.Object);
            mockServiceProvider.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(mockOptions.Object);
            mockServiceProvider.Setup(sp => sp.GetKeyedService(typeof(IVectorSearchable<string>), vectorSearchServiceId)).Returns(mockVectorSearchable.Object);

            services.AddSingleton<IServiceProvider>(mockServiceProvider.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<string>(
                    vectorSearchServiceId,
                    textEmbeddingGenerationServiceId,
                    mockStringMapper.Object,
                    mockResultMapper.Object,
                    mockOptions.Object));
        }
    }
}
