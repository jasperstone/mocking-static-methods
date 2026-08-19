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
        public void AddVectorStoreTextSearch_ShouldRegisterVectorStoreTextSearch()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var optionsMock = new VectorStoreTextSearchOptions();

            // Act
            services.AddVectorStoreTextSearch<string>(
                stringMapperMock.Object,
                resultMapperMock.Object,
                optionsMock);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var vectorStoreTextSearch = serviceProvider.GetService<VectorStoreTextSearch<string>>();
            Assert.NotNull(vectorStoreTextSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_ShouldThrowInvalidOperationException_WhenIVectorSearchableIsNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var optionsMock = new VectorStoreTextSearchOptions();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<string>(
                    stringMapperMock.Object,
                    resultMapperMock.Object,
                    optionsMock));
            Assert.Equal("No IVectorSearch<TRecord> registered.", exception.Message);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithServiceId_ShouldRegisterVectorStoreTextSearch()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var optionsMock = new VectorStoreTextSearchOptions();
            var vectorSearchableServiceId = "vectorSearchableServiceId";

            // Act
            services.AddVectorStoreTextSearch<string>(
                vectorSearchableServiceId,
                stringMapperMock.Object,
                resultMapperMock.Object,
                optionsMock);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var vectorStoreTextSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<string>>(vectorSearchableServiceId);
            Assert.NotNull(vectorStoreTextSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithServiceId_ShouldThrowInvalidOperationException_WhenIVectorSearchableIsNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var optionsMock = new VectorStoreTextSearchOptions();
            var vectorSearchableServiceId = "vectorSearchableServiceId";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<string>(
                    vectorSearchableServiceId,
                    stringMapperMock.Object,
                    resultMapperMock.Object,
                    optionsMock));
            Assert.Equal($"No IVectorSearch<TRecord> for service id {vectorSearchableServiceId} registered.", exception.Message);
        }
    }
}
