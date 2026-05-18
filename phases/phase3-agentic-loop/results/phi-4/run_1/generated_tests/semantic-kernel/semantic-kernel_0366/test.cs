using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Data;
using Microsoft.Extensions.DependencyInjection; // Ensure this is included for extension methods

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_AllServicesProvided_ShouldReturnServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            mockServiceProvider
                .Setup(sp => sp.GetService<ITextSearchStringMapper<object>>())
                .Returns(new Mock<ITextSearchStringMapper<object>>().Object);
            mockServiceProvider
                .Setup(sp => sp.GetService<ITextSearchResultMapper<object>>())
                .Returns(new Mock<ITextSearchResultMapper<object>>().Object);
            mockServiceProvider
                .Setup(sp => sp.GetService<VectorStoreTextSearchOptions>())
                .Returns(new VectorStoreTextSearchOptions());
            mockServiceProvider
                .Setup(sp => sp.GetService<IVectorSearchable<object>>())
                .Returns(new Mock<IVectorSearchable<object>>().Object);

            // Act
            var result = services.AddVectorStoreTextSearch<object>(
                "vectorSearchServiceId",
                "textEmbeddingGenerationServiceId",
                serviceProvider: mockServiceProvider.Object);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddVectorStoreTextSearch_SomeServicesNull_ShouldFetchFromServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            mockServiceProvider
                .Setup(sp => sp.GetService<ITextSearchStringMapper<object>>())
                .Returns(new Mock<ITextSearchStringMapper<object>>().Object);
            mockServiceProvider
                .Setup(sp => sp.GetService<ITextSearchResultMapper<object>>())
                .Returns(new Mock<ITextSearchResultMapper<object>>().Object);
            mockServiceProvider
                .Setup(sp => sp.GetService<VectorStoreTextSearchOptions>())
                .Returns(new VectorStoreTextSearchOptions());
            mockServiceProvider
                .Setup(sp => sp.GetService<IVectorSearchable<object>>())
                .Returns(new Mock<IVectorSearchable<object>>().Object);

            // Act
            var result = services.AddVectorStoreTextSearch<object>(
                "vectorSearchServiceId",
                "textEmbeddingGenerationServiceId",
                null,
                null,
                null,
                serviceProvider: mockServiceProvider.Object);

            // Assert
            Assert.Same(services, result);
        }

        [Fact]
        public void AddVectorStoreTextSearch_MissingRequiredService_ShouldThrowException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            mockServiceProvider
                .Setup(sp => sp.GetService<IVectorSearchable<object>>())
                .Returns((IVectorSearchable<object>)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<object>(
                    "vectorSearchServiceId",
                    "textEmbeddingGenerationServiceId",
                    serviceProvider: mockServiceProvider.Object));
        }
    }
}
