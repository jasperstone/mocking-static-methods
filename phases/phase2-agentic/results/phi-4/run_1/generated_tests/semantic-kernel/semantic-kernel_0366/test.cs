using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_SuccessfullyRegistersService_WhenAllDependenciesAreAvailable()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var mockVectorSearch = new Mock<IVectorSearchable<object>>();
            var mockGenerationService = new Mock<ITextEmbeddingGenerationService>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();

            services.AddKeyedService("vectorSearchServiceId", mockVectorSearch.Object);
            services.AddKeyedService("textEmbeddingGenerationServiceId", mockGenerationService.Object);
            services.AddSingleton(mockStringMapper.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(mockOptions.Object);

            // Act
            services.AddVectorStoreTextSearch<object>(
                "vectorSearchServiceId",
                "textEmbeddingGenerationServiceId",
                null,
                null,
                null,
                "serviceId");

            // Assert
            var service = services.BuildServiceProvider().GetKeyedService<VectorStoreTextSearch<object>>("serviceId");
            Assert.NotNull(service);
        }

        [Fact]
        public void AddVectorStoreTextSearch_ThrowsInvalidOperationException_WhenVectorSearchServiceIsNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<object>(
                    "nonExistentVectorSearchServiceId",
                    "textEmbeddingGenerationServiceId",
                    null,
                    null,
                    null,
                    "serviceId"));
        }

        [Fact]
        public void AddVectorStoreTextSearch_ThrowsInvalidOperationException_WhenGenerationServiceIsNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var mockVectorSearch = new Mock<IVectorSearchable<object>>();
            services.AddKeyedService("vectorSearchServiceId", mockVectorSearch.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<object>(
                    "vectorSearchServiceId",
                    "nonExistentGenerationServiceId",
                    null,
                    null,
                    null,
                    "serviceId"));
        }
    }
}
