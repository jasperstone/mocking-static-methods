using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        private IServiceCollection CreateServiceCollection()
        {
            var services = new ServiceCollection();
            // Register mock services
            services.AddTransient<ITextSearchStringMapper, MockStringMapper>();
            services.AddTransient<ITextSearchResultMapper, MockResultMapper>();
            services.AddTransient<VectorStoreTextSearchOptions, MockOptions>();
            services.AddTransient<IVectorSearchable<MockRecord>, MockVectorSearch>();
            services.AddTransient<ITextEmbeddingGenerationService, MockEmbeddingService>();
            return services;
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithServiceId_ShouldResolveAndCreate()
        {
            var services = CreateServiceCollection();

            // Register a keyed service for IVectorSearchable<MockRecord>
            var vectorSearch = new MockVectorSearch();
            services.AddKeyedTransient<IVectorSearchable<MockRecord>>("testVector", (sp, o) => vectorSearch);

            // Register a keyed service for ITextEmbeddingGenerationService
            var embeddingService = new MockEmbeddingService();
            services.AddKeyedTransient<ITextEmbeddingGenerationService>("testEmbedding", (sp, o) => embeddingService);

            // Build service provider
            var provider = services.BuildServiceProvider();

            // Call extension method
            services.AddVectorStoreTextSearch<MockRecord>(
                "testVector",
                "testEmbedding",
                null,
                null,
                null,
                "testService");

            var sp = services.BuildServiceProvider();

            // Resolve the registered VectorStoreTextSearch
            var result = sp.GetService<VectorStoreTextSearch<MockRecord>>();

            Assert.NotNull(result);
            Assert.IsType<VectorStoreTextSearch<MockRecord>>(result);
            Assert.Equal(vectorSearch, result.VectorSearch);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithServiceId_VectorSearchNotRegistered_ShouldThrow()
        {
            var services = new ServiceCollection();

            // Register only embedding service
            services.AddKeyedTransient<ITextEmbeddingGenerationService>("testEmbedding", (sp, o) => new MockEmbeddingService());

            var provider = services.BuildServiceProvider();

            // Register the service with extension method
            services.AddVectorStoreTextSearch<MockRecord>(
                "nonexistent",
                "testEmbedding",
                null,
                null,
                null,
                "testService");

            var sp = services.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => sp.GetService<VectorStoreTextSearch<MockRecord>>());
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithServiceId_EmbeddingServiceNotRegistered_ShouldThrow()
        {
            var services = new ServiceCollection();

            // Register only vector search
            services.AddKeyedTransient<IVectorSearchable<MockRecord>>("testVector", (sp, o) => new MockVectorSearch());

            var provider = services.BuildServiceProvider();

            // Register the service with extension method
            services.AddVectorStoreTextSearch<MockRecord>(
                "testVector",
                "nonexistentEmbedding",
                null,
                null,
                null,
                "testService");

            var sp = services.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => sp.GetService<VectorStoreTextSearch<MockRecord>>());
        }

        // Mock classes for testing
        private class MockRecord { }
        private class MockStringMapper : ITextSearchStringMapper { }
        private class MockResultMapper : ITextSearchResultMapper { }
        private class MockOptions : VectorStoreTextSearchOptions { }
        private class MockVectorSearch : IVectorSearchable<MockRecord> { }
        private class MockEmbeddingService : ITextEmbeddingGenerationService { }
    }
}
