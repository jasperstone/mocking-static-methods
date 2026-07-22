using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        private class FakeEmbeddingGenerator : IEmbeddingGenerator, IDisposable
        {
            public string ModelId => "fake-model";
            public int Dimensions => 1;

            public float[] GetEmbedding(string input) => new float[] { 0.1f };

            public Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default) =>
                Task.FromResult(new float[] { 0.1f });

            // Explicit interface implementation to satisfy the interface
            object? IServiceProvider.GetService(Type serviceType) => null;

            public void Dispose()
            {
                // No resources to dispose
            }
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithEmbeddingGenerator()
        {
            var services = new ServiceCollection();
            var embeddingGenerator = new FakeEmbeddingGenerator();
            var optionsWithEmbedding = new QdrantCollectionOptions { EmbeddingGenerator = embeddingGenerator };
            services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
            var sp = services.BuildServiceProvider();

            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => optionsWithEmbedding;

            var result = InvokeGetCollectionOptions(sp, optionsProvider);

            Assert.Same(optionsWithEmbedding, result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithoutEmbeddingGenerator_AndNoEmbeddingGeneratorInServiceProvider()
        {
            var services = new ServiceCollection();
            var optionsWithoutEmbedding = new QdrantCollectionOptions { EmbeddingGenerator = null };
            var sp = services.BuildServiceProvider();

            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => optionsWithoutEmbedding;

            var result = InvokeGetCollectionOptions(sp, optionsProvider);

            Assert.Same(optionsWithoutEmbedding, result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderReturnsOptionsWithoutEmbeddingGenerator_AndEmbeddingGeneratorInServiceProvider()
        {
            var services = new ServiceCollection();
            var embeddingGenerator = new FakeEmbeddingGenerator();
            services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
            var optionsWithoutEmbedding = new QdrantCollectionOptions { EmbeddingGenerator = null };
            var sp = services.BuildServiceProvider();

            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => optionsWithoutEmbedding;

            var result = InvokeGetCollectionOptions(sp, optionsProvider);

            Assert.NotNull(result);
            Assert.NotSame(optionsWithoutEmbedding, result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNull_WhenOptionsProviderIsNull_AndNoEmbeddingGeneratorInServiceProvider()
        {
            var services = new ServiceCollection();
            var sp = services.BuildServiceProvider();

            Func<IServiceProvider, QdrantCollectionOptions?>? optionsProvider = null;

            var result = InvokeGetCollectionOptions(sp, optionsProvider);

            Assert.Null(result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderIsNull_AndEmbeddingGeneratorInServiceProvider()
        {
            var services = new ServiceCollection();
            var embeddingGenerator = new FakeEmbeddingGenerator();
            services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
            var sp = services.BuildServiceProvider();

            Func<IServiceProvider, QdrantCollectionOptions?>? optionsProvider = null;

            var result = InvokeGetCollectionOptions(sp, optionsProvider);

            Assert.NotNull(result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithEmbeddingGenerator()
        {
            var services = new ServiceCollection();
            var embeddingGenerator = new FakeEmbeddingGenerator();
            var optionsWithEmbedding = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGenerator };
            services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
            var sp = services.BuildServiceProvider();

            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => optionsWithEmbedding;

            var result = InvokeGetStoreOptions(sp, optionsProvider);

            Assert.Same(optionsWithEmbedding, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithoutEmbeddingGenerator_AndNoEmbeddingGeneratorInServiceProvider()
        {
            var services = new ServiceCollection();
            var optionsWithoutEmbedding = new QdrantVectorStoreOptions { EmbeddingGenerator = null };
            var sp = services.BuildServiceProvider();

            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => optionsWithoutEmbedding;

            var result = InvokeGetStoreOptions(sp, optionsProvider);

            Assert.Same(optionsWithoutEmbedding, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderReturnsOptionsWithoutEmbeddingGenerator_AndEmbeddingGeneratorInServiceProvider()
        {
            var services = new ServiceCollection();
            var embeddingGenerator = new FakeEmbeddingGenerator();
            services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
            var optionsWithoutEmbedding = new QdrantVectorStoreOptions { EmbeddingGenerator = null };
            var sp = services.BuildServiceProvider();

            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => optionsWithoutEmbedding;

            var result = InvokeGetStoreOptions(sp, optionsProvider);

            Assert.NotNull(result);
            Assert.NotSame(optionsWithoutEmbedding, result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenOptionsProviderIsNull_AndNoEmbeddingGeneratorInServiceProvider()
        {
            var services = new ServiceCollection();
            var sp = services.BuildServiceProvider();

            Func<IServiceProvider, QdrantVectorStoreOptions?>? optionsProvider = null;

            var result = InvokeGetStoreOptions(sp, optionsProvider);

            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderIsNull_AndEmbeddingGeneratorInServiceProvider()
        {
            var services = new ServiceCollection();
            var embeddingGenerator = new FakeEmbeddingGenerator();
            services.AddSingleton<IEmbeddingGenerator>(embeddingGenerator);
            var sp = services.BuildServiceProvider();

            Func<IServiceProvider, QdrantVectorStoreOptions?>? optionsProvider = null;

            var result = InvokeGetStoreOptions(sp, optionsProvider);

            Assert.NotNull(result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        private static QdrantCollectionOptions? InvokeGetCollectionOptions(IServiceProvider sp, Func<IServiceProvider, QdrantCollectionOptions?>? optionsProvider)
        {
            var type = typeof(QdrantServiceCollectionExtensions);
            var method = type.GetMethod("GetCollectionOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (QdrantCollectionOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }

        private static QdrantVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, QdrantVectorStoreOptions?>? optionsProvider)
        {
            var type = typeof(QdrantServiceCollectionExtensions);
            var method = type.GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (QdrantVectorStoreOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
