using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
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
            var stringMapper = Mock.Of<Microsoft.SemanticKernel.Data.ITextSearchStringMapper>();
            var resultMapper = Mock.Of<Microsoft.SemanticKernel.Data.ITextSearchResultMapper>();
            var options = new Microsoft.SemanticKernel.Data.VectorStoreTextSearchOptions();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddVectorStoreTextSearch<object>(stringMapper, resultMapper, options));
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithValidVectorSearchable_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapper = Mock.Of<Microsoft.SemanticKernel.Data.ITextSearchStringMapper>();
            var resultMapper = Mock.Of<Microsoft.SemanticKernel.Data.ITextSearchResultMapper>();
            var options = new Microsoft.SemanticKernel.Data.VectorStoreTextSearchOptions();
            var vectorSearchable = Mock.Of<Microsoft.SemanticKernel.Data.IVectorSearchable<object>>();

            services.AddSingleton(vectorSearchable);

            // Act and Assert
            services.AddVectorStoreTextSearch<object>(stringMapper, resultMapper, options, serviceId: "test");
        }
    }
}
