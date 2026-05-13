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
            var stringMapper = new Mock<ITextSearchStringMapper>().Object;
            var resultMapper = new Mock<ITextSearchResultMapper>().Object;
            var options = new VectorStoreTextSearchOptions();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddVectorStoreTextSearch<object>(stringMapper, resultMapper, options));
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithValidVectorSearchable_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapper = new Mock<ITextSearchStringMapper>().Object;
            var resultMapper = new Mock<ITextSearchResultMapper>().Object;
            var options = new VectorStoreTextSearchOptions();
            var vectorSearchable = new Mock<IVectorSearchable<object>>().Object;

            services.AddSingleton<IVectorSearchable<object>>(vectorSearchable);

            // Act and Assert
            services.AddVectorStoreTextSearch<object>(stringMapper, resultMapper, options);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithNullStringMapper_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var resultMapper = new Mock<ITextSearchResultMapper>().Object;
            var options = new VectorStoreTextSearchOptions();
            var vectorSearchable = new Mock<IVectorSearchable<object>>().Object;

            services.AddSingleton<IVectorSearchable<object>>(vectorSearchable);

            // Act and Assert
            services.AddVectorStoreTextSearch<object>(null, resultMapper, options);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithNullResultMapper_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapper = new Mock<ITextSearchStringMapper>().Object;
            var options = new VectorStoreTextSearchOptions();
            var vectorSearchable = new Mock<IVectorSearchable<object>>().Object;

            services.AddSingleton<IVectorSearchable<object>>(vectorSearchable);

            // Act and Assert
            services.AddVectorStoreTextSearch<object>(stringMapper, null, options);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithNullOptions_DoesNotThrow()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapper = new Mock<ITextSearchStringMapper>().Object;
            var resultMapper = new Mock<ITextSearchResultMapper>().Object;
            var vectorSearchable = new Mock<IVectorSearchable<object>>().Object;

            services.AddSingleton<IVectorSearchable<object>>(vectorSearchable);

            // Act and Assert
            services.AddVectorStoreTextSearch<object>(stringMapper, resultMapper, null);
        }
    }
}
