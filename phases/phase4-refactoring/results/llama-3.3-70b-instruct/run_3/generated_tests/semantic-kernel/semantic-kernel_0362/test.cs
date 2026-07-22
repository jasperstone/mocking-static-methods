using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Moq;
using Xunit;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_WithNullVectorSearchable_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapper = Mock.Of<ITextSearchStringMapper>();
            var resultMapper = Mock.Of<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddVectorStoreTextSearch<MyRecord>(stringMapper, resultMapper, options));
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithValidVectorSearchable_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapper = Mock.Of<ITextSearchStringMapper>();
            var resultMapper = Mock.Of<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();
            var vectorSearchable = Mock.Of<IVectorSearchable<MyRecord>>();

            services.AddSingleton<IVectorSearchable<MyRecord>>(vectorSearchable);

            // Act and Assert
            services.AddVectorStoreTextSearch<MyRecord>(stringMapper, resultMapper, options);
        }

        private class MyRecord
        {
        }
    }
}
