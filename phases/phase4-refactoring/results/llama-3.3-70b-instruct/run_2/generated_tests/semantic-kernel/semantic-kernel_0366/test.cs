using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Moq;
using Xunit;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_WithNullStringMapper_ResultMapperAndOptions_ResolvesFromServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var optionsMock = new Mock<VectorStoreTextSearchOptions>();

            services.AddSingleton<ITextSearchStringMapper>(stringMapperMock.Object);
            services.AddSingleton<ITextSearchResultMapper>(resultMapperMock.Object);
            services.AddSingleton<VectorStoreTextSearchOptions>(optionsMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddVectorStoreTextSearch<MyRecord>(stringMapperMock.Object, resultMapperMock.Object, optionsMock.Object);

            // Assert
            var serviceProvider2 = services.BuildServiceProvider();
            var textSearch = serviceProvider2.GetService<VectorStoreTextSearch<MyRecord>>();

            Assert.NotNull(textSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithNullVectorSearchable_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var optionsMock = new Mock<VectorStoreTextSearchOptions>();

            services.AddSingleton<ITextSearchStringMapper>(stringMapperMock.Object);
            services.AddSingleton<ITextSearchResultMapper>(resultMapperMock.Object);
            services.AddSingleton<VectorStoreTextSearchOptions>(optionsMock.Object);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddVectorStoreTextSearch<MyRecord>("vectorSearchServiceId", "textEmbeddingGenerationServiceId"));
        }

        private class MyRecord
        {
        }
    }
}
