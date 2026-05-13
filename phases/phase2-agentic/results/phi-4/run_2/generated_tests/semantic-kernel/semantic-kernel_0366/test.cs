using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_RegistersService_WhenAllServicesAreAvailable()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var mockVectorSearchable = new Mock<IVectorSearchable<object>>();
            var mockGenerationService = new Mock<ITextEmbeddingGenerationService>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();

            services.AddSingleton(mockVectorSearchable.Object);
            services.AddSingleton(mockGenerationService.Object);
            services.AddSingleton(mockStringMapper.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(mockOptions.Object);

            // Act
            services.AddVectorStoreTextSearch<object>("vectorSearch", "generationService");

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<VectorStoreTextSearch<object>>();
            Assert.NotNull(service);
        }

        [Fact]
        public void AddVectorStoreTextSearch_UsesServiceProviderForStringMapper_WhenNotProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var mockVectorSearchable = new Mock<IVectorSearchable<object>>();
            var mockGenerationService = new Mock<ITextEmbeddingGenerationService>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();

            services.AddSingleton(mockVectorSearchable.Object);
            services.AddSingleton(mockGenerationService.Object);
            services.AddSingleton(mockStringMapper.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(mockOptions.Object);

            // Act
            services.AddVectorStoreTextSearch<object>("vectorSearch", "generationService");

            // Assert
            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<VectorStoreTextSearch<object>>();
            Assert.NotNull(service);
            mockStringMapper.Verify(s => s.MapFromResultToString(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public void AddVectorStoreTextSearch_Throws_WhenVectorSearchableNotRegistered()
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
                services.AddVectorStoreTextSearch<object>("vectorSearch", "generationService"));
        }

        [Fact]
        public void AddVectorStoreTextSearch_Throws_WhenGenerationServiceNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var mockVectorSearchable = new Mock<IVectorSearchable<object>>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();

            services.AddSingleton(mockVectorSearchable.Object);
            services.AddSingleton(mockStringMapper.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(mockOptions.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<object>("vectorSearch", "generationService"));
        }
    }
}
