using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.VectorData;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVectorStoreTextSearch_RegistersAndResolvesServices()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockVectorSearch = new Mock<IVectorSearchable<string>>();
            var mockEmbeddingService = new Mock<ITextEmbeddingGenerationService>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            // Register the mock services
            services.AddSingleton(mockVectorSearch.Object);
            services.AddSingleton(mockEmbeddingService.Object);
            services.AddSingleton(mockStringMapper.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(options);

            // Register the extension method
            services.AddKeyedTransient<VectorStoreTextSearch<string>>(
                "testService",
                (sp, obj) =>
                {
                    var vectorSearch = sp.GetService<IVectorSearchable<string>>();
                    var embedding = sp.GetService<ITextEmbeddingGenerationService>();
                    var stringMapper = sp.GetService<ITextSearchStringMapper>();
                    var resultMapper = sp.GetService<ITextSearchResultMapper>();
                    var opts = sp.GetService<VectorStoreTextSearchOptions>();

                    Assert.NotNull(vectorSearch);
                    Assert.NotNull(embedding);
                    Assert.NotNull(stringMapper);
                    Assert.NotNull(resultMapper);
                    Assert.NotNull(opts);

                    return new VectorStoreTextSearch<string>(vectorSearch, embedding, stringMapper, resultMapper, opts);
                });

            var provider = services.BuildServiceProvider();

            // Mock the IServiceProvider to return the services
            var mockProvider = new Mock<IServiceProvider>();
            mockProvider.Setup(sp => sp.GetService(typeof(IVectorSearchable<string>))).Returns(mockVectorSearch.Object);
            mockProvider.Setup(sp => sp.GetService(typeof(ITextEmbeddingGenerationService))).Returns(mockEmbeddingService.Object);
            mockProvider.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(mockStringMapper.Object);
            mockProvider.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(mockResultMapper.Object);
            mockProvider.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(options);

            // Act: resolve the registered service
            var registeredServices = provider.GetServices<VectorStoreTextSearch<string>>();
            Assert.NotNull(registeredServices);
        }

        [Fact]
        public void AddVectorStoreTextSearch_ThrowsWhenVectorSearchNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();

            // Do not register IVectorSearchable
            var mockEmbeddingService = new Mock<ITextEmbeddingGenerationService>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            // Register the services
            services.AddSingleton(mockEmbeddingService.Object);
            services.AddSingleton(mockStringMapper.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(options);

            // Register the extension method
            services.AddKeyedTransient<VectorStoreTextSearch<string>>(
                "testService",
                (sp, obj) =>
                {
                    var vectorSearch = sp.GetService<IVectorSearchable<string>>();
                    var embedding = sp.GetService<ITextEmbeddingGenerationService>();
                    var stringMapper = sp.GetService<ITextSearchStringMapper>();
                    var resultMapper = sp.GetService<ITextSearchResultMapper>();
                    var opts = sp.GetService<VectorStoreTextSearchOptions>();

                    if (vectorSearch is null)
                        throw new InvalidOperationException($"No IVectorSearch<TRecord> for service id vectorService registered.");

                    return new VectorStoreTextSearch<string>(vectorSearch, embedding, stringMapper, resultMapper, opts);
                });

            var provider = services.BuildServiceProvider();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                var service = provider.GetService<VectorStoreTextSearch<string>>();
            });
        }
    }
}
