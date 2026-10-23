using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace QdrantServiceCollectionExtensionsTests
{
    public class GetServiceTests
    {
        private class DummyService { }
        private class DummyEmbeddingGenerator { }

        [Fact]
        public void GetService_ReturnsEmbeddingGenerator_WhenRegistered()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IEmbeddingGenerator, DummyEmbeddingGenerator>();
            var provider = services.BuildServiceProvider();

            // Directly test the line in GetStoreOptions that calls GetService<IEmbeddingGenerator>()
            var optionsProvider = (Func<IServiceProvider, QdrantVectorStoreOptions?>?)null;
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(provider, optionsProvider);

            Assert.NotNull(options);
            Assert.IsType<DummyEmbeddingGenerator>(options!.EmbeddingGenerator);
        }
    }
}
