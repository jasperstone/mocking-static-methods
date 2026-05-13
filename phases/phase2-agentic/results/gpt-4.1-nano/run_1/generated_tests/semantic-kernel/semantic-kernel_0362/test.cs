using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        private class DummyVectorSearch : IVectorSearchable<int>
        {
            public int[] Data => new[] { 1, 2, 3 };
        }

        private class DummyStringMapper : ITextSearchStringMapper { }
        private class DummyResultMapper : ITextSearchResultMapper { }
        private class DummyOptions : VectorStoreTextSearchOptions { }

        [Fact]
        public void AddVectorStoreTextSearch_Should_Resolve_IVectorSearchable()
        {
            var services = new ServiceCollection();
            services.AddTransient<IVectorSearchable<int>, DummyVectorSearch>();
            services.AddTransient<ITextSearchStringMapper, DummyStringMapper>();
            services.AddTransient<ITextSearchResultMapper, DummyResultMapper>();
            services.AddTransient<VectorStoreTextSearchOptions, DummyOptions>();

            var provider = services.BuildServiceProvider();

            var newServices = new ServiceCollection();
            newServices.AddVectorStoreTextSearch<int>(sp => sp.GetService<IVectorSearchable<int>>());

            var sp2 = newServices.BuildServiceProvider();

            var result = sp2.GetService<VectorStoreTextSearch<int>>();
            Assert.NotNull(result);
        }

        [Fact]
        public void AddVectorStoreTextSearch_Should_Throw_If_IVectorSearchable_Not_Registered()
        {
            var services = new ServiceCollection();
            services.AddTransient<ITextSearchStringMapper, DummyStringMapper>();
            services.AddTransient<ITextSearchResultMapper, DummyResultMapper>();
            services.AddTransient<VectorStoreTextSearchOptions, DummyOptions>();

            var provider = services.BuildServiceProvider();

            var newServices = new ServiceCollection();
            newServices.AddVectorStoreTextSearch<int>(sp => sp.GetService<IVectorSearchable<int>>());

            var sp2 = newServices.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => sp2.GetService<VectorStoreTextSearch<int>>());
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithServiceId_Should_Resolve_IVectorSearchable()
        {
            var services = new ServiceCollection();
            services.AddTransient<IVectorSearchable<int>, DummyVectorSearch>();
            services.AddTransient<ITextSearchStringMapper, DummyStringMapper>();
            services.AddTransient<ITextSearchResultMapper, DummyResultMapper>();
            services.AddTransient<VectorStoreTextSearchOptions, DummyOptions>();

            var provider = services.BuildServiceProvider();

            var newServices = new ServiceCollection();
            newServices.AddVectorStoreTextSearch<int>("default", sp => sp.GetKeyedService<IVectorSearchable<int>>("default"));

            var sp2 = newServices.BuildServiceProvider();

            var result = sp2.GetService<VectorStoreTextSearch<int>>();
            Assert.NotNull(result);
        }

        [Fact]
        public void AddVectorStoreTextSearch_WithServiceId_Should_Throw_If_IVectorSearchable_Not_Registered()
        {
            var services = new ServiceCollection();
            services.AddTransient<ITextSearchStringMapper, DummyStringMapper>();
            services.AddTransient<ITextSearchResultMapper, DummyResultMapper>();
            services.AddTransient<VectorStoreTextSearchOptions, DummyOptions>();

            var provider = services.BuildServiceProvider();

            var newServices = new ServiceCollection();
            newServices.AddVectorStoreTextSearch<int>("nonexistent", sp => sp.GetKeyedService<IVectorSearchable<int>>("nonexistent"));

            var sp2 = newServices.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => sp2.GetService<VectorStoreTextSearch<int>>());
        }
    }
}
