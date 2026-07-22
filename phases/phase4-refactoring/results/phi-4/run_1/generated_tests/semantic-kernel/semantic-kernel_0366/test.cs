using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_AllServicesAvailable_CreatesInstance()
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
            services.AddVectorStoreTextSearch<object>("vectorSearchId", "generationServiceId");

            // Assert
            var provider = services.BuildServiceProvider();
            var textSearch = provider.GetRequiredService<VectorStoreTextSearch<object>>();
            Assert.NotNull(textSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_StringMapperNotAvailable_UsesServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var mockVectorSearch = new Mock<IVectorSearchable<object>>();
            var mockGenerationService = new Mock<ITextEmbeddingGenerationService>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();

            services.AddSingleton(mockVectorSearch.Object);
            services.AddSingleton(mockGenerationService.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(mockOptions.Object);

            // Act
            services.AddVectorStoreTextSearch<object>("vectorSearchId", "generationServiceId");

            // Assert
            var provider = services.BuildServiceProvider();
            var textSearch = provider.GetRequiredService<VectorStoreTextSearch<object>>();
            Assert.NotNull(textSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_VectorSearchNotAvailable_ThrowsException()
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
            {
                services.AddVectorStoreTextSearch<object>("vectorSearchId", "generationServiceId");
            });
        }

        [Fact]
        public void AddVectorStoreTextSearch_GenerationServiceNotAvailable_ThrowsException()
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
            {
                services.AddVectorStoreTextSearch<object>("vectorSearchId", "generationServiceId");
            });
        }
    }
}
