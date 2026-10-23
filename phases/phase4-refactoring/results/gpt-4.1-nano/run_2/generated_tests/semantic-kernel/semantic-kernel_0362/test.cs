using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using System;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_WithServiceProvider_ReturnsServicesAndCallsGetService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();
            var mockVectorSearchable = new Mock<IVectorSearchable<int>>();

            services.AddTransient(_ => mockVectorSearchable.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = services.AddVectorStoreTextSearch<int>(
                stringMapper: mockStringMapper.Object,
                resultMapper: mockResultMapper.Object,
                options: mockOptions.Object);

            var sp = result.BuildServiceProvider();

            // Assert
            var vectorSearch = sp.GetService<IVectorSearchable<int>>();
            Assert.NotNull(vectorSearch);
            Assert.Equal(mockVectorSearchable.Object, vectorSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithServiceId_CallsGetKeyedService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();
            var mockVectorSearchable = new Mock<IVectorSearchable<string>>();

            var serviceId = "testService";

            services.AddTransient(_ => mockVectorSearchable.Object);

            var sp = services.BuildServiceProvider();

            // Act
            var result = services.AddVectorStoreTextSearch<string>(
                serviceId,
                stringMapper: mockStringMapper.Object,
                resultMapper: mockResultMapper.Object,
                options: mockOptions.Object);

            var sp2 = result.BuildServiceProvider();

            // Assert
            var vectorSearch = sp2.GetService<IVectorSearchable<string>>();
            Assert.NotNull(vectorSearch);
            Assert.Equal(mockVectorSearchable.Object, vectorSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithServiceIdAndMissingService_Throws()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var mockOptions = new Mock<VectorStoreTextSearchOptions>();

            var serviceId = "missingService";

            // No services added for IVectorSearchable<string>

            var result = services.AddVectorStoreTextSearch<string>(
                serviceId,
                stringMapper: mockStringMapper.Object,
                resultMapper: mockResultMapper.Object,
                options: mockOptions.Object);

            var sp = result.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => sp.GetService<IVectorSearchable<string>>());
        }
    }
}
