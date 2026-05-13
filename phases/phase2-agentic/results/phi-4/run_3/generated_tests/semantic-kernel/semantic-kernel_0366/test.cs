using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_AllServicesAvailable_CreatesVectorStoreTextSearch()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var mockVectorSearch = new Mock<IVectorSearchable<object>>();
            var mockGenerationService = new Mock<ITextEmbeddingGenerationService>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();

            services.AddSingleton(mockVectorSearch.Object);
            services.AddSingleton(mockGenerationService.Object);
            services.AddSingleton(mockStringMapper.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(mockOptions.Object);

            // Act
            services.AddVectorStoreTextSearch<object>("vectorSearch", "generationService", null, null, null, "serviceId");

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<VectorStoreTextSearch<object>>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddVectorStoreTextSearch_StringMapperNotProvided_GetServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var mockVectorSearch = new Mock<IVectorSearchable<object>>();
            var mockGenerationService = new Mock<ITextEmbeddingGenerationService>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();

            services.AddSingleton(mockVectorSearch.Object);
            services.AddSingleton(mockGenerationService.Object);
            services.AddSingleton(mockStringMapper.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(mockOptions.Object);

            // Act
            services.AddVectorStoreTextSearch<object>("vectorSearch", "generationService", null, null, null, "serviceId");

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<VectorStoreTextSearch<object>>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddVectorStoreTextSearch_VectorSearchNotRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var mockGenerationService = new Mock<ITextEmbeddingGenerationService>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();

            services.AddSingleton(mockGenerationService.Object);
            services.AddSingleton(mockStringMapper.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(mockOptions.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<object>("vectorSearch", "generationService", null, null, null, "serviceId"));
        }

        [Fact]
        public void AddVectorStoreTextSearch_GenerationServiceNotRegistered_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var mockVectorSearch = new Mock<IVectorSearchable<object>>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();

            services.AddSingleton(mockVectorSearch.Object);
            services.AddSingleton(mockStringMapper.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(mockOptions.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<object>("vectorSearch", "generationService", null, null, null, "serviceId"));
        }
    }
}
