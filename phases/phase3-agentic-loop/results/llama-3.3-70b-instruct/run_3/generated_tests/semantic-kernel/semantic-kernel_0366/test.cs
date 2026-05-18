using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_ServiceProvider_GetService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var vectorSearchServiceId = "vectorSearchServiceId";
            var textEmbeddingGenerationServiceId = "textEmbeddingGenerationServiceId";
            var stringMapper = new Mock<ITextSearchStringMapper>();
            var resultMapper = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            services.AddKeyedTransient<IVectorSearchable<object>>(vectorSearchServiceId, (sp, obj) => new Mock<IVectorSearchable<object>>().Object);
            services.AddKeyedTransient<ITextEmbeddingGenerationService>(textEmbeddingGenerationServiceId, (sp, obj) => new Mock<ITextEmbeddingGenerationService>().Object);
            services.AddKeyedTransient<ITextSearchStringMapper>(stringMapper.Object);
            services.AddKeyedTransient<ITextSearchResultMapper>(resultMapper.Object);
            services.AddKeyedTransient<VectorStoreTextSearchOptions>(options);

            // Act
            services.AddVectorStoreTextSearch<object>(vectorSearchServiceId, textEmbeddingGenerationServiceId, stringMapper.Object, resultMapper.Object, options);

            // Assert
            var serviceProvider2 = services.BuildServiceProvider();
            var vectorStoreTextSearch = serviceProvider2.GetService<VectorStoreTextSearch<object>>();

            Assert.NotNull(vectorStoreTextSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_ServiceProvider_GetService_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();
            var vectorSearchServiceId = "vectorSearchServiceId";
            var textEmbeddingGenerationServiceId = "textEmbeddingGenerationServiceId";
            var stringMapper = new Mock<ITextSearchStringMapper>();
            var resultMapper = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            services.AddKeyedTransient<IVectorSearchable<object>>(vectorSearchServiceId, (sp, obj) => new Mock<IVectorSearchable<object>>().Object);
            services.AddKeyedTransient<ITextSearchStringMapper>(stringMapper.Object);
            services.AddKeyedTransient<ITextSearchResultMapper>(resultMapper.Object);
            services.AddKeyedTransient<VectorStoreTextSearchOptions>(options);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddVectorStoreTextSearch<object>(vectorSearchServiceId, textEmbeddingGenerationServiceId, stringMapper.Object, resultMapper.Object, options));
        }
    }
}
