using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    // Minimal interface definition to allow testing
    public interface IEmbeddingGenerator { }

    // Minimal QdrantCollectionOptions class with EmbeddingGenerator property
    public class QdrantCollectionOptions
    {
        public IEmbeddingGenerator? EmbeddingGenerator { get; set; }

        public QdrantCollectionOptions() { }

        public QdrantCollectionOptions(QdrantCollectionOptions other)
        {
            if (other != null)
            {
                this.EmbeddingGenerator = other.EmbeddingGenerator;
            }
        }
    }

    public class QdrantServiceCollectionExtensionsTests
    {
        private class FakeEmbeddingGenerator : IEmbeddingGenerator { }

        private class SimpleServiceProvider : IServiceProvider
        {
            private readonly IEmbeddingGenerator? _embeddingGenerator;

            public SimpleServiceProvider(IEmbeddingGenerator? embeddingGenerator)
            {
                _embeddingGenerator = embeddingGenerator;
            }

            public object? GetService(Type serviceType)
            {
                if (serviceType == typeof(IEmbeddingGenerator))
                {
                    return _embeddingGenerator;
                }
                return null;
            }
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithEmbeddingGenerator()
        {
            var embeddingGenerator = new FakeEmbeddingGenerator();
            var optionsWithEmbedding = new QdrantCollectionOptions { EmbeddingGenerator = embeddingGenerator };
            var sp = new SimpleServiceProvider(embeddingGenerator);

            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => optionsWithEmbedding;

            var result = InvokeGetCollectionOptions(sp, optionsProvider);

            Assert.Same(optionsWithEmbedding, result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithoutEmbeddingGenerator_AndNoEmbeddingGeneratorInServiceProvider()
        {
            var optionsWithoutEmbedding = new QdrantCollectionOptions { EmbeddingGenerator = null };
            var sp = new SimpleServiceProvider(null);

            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => optionsWithoutEmbedding;

            var result = InvokeGetCollectionOptions(sp, optionsProvider);

            Assert.Same(optionsWithoutEmbedding, result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderReturnsOptionsWithoutEmbeddingGenerator_AndEmbeddingGeneratorInServiceProvider()
        {
            var embeddingGenerator = new FakeEmbeddingGenerator();
            var optionsWithoutEmbedding = new QdrantCollectionOptions { EmbeddingGenerator = null };
            var sp = new SimpleServiceProvider(embeddingGenerator);

            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => optionsWithoutEmbedding;

            var result = InvokeGetCollectionOptions(sp, optionsProvider);

            Assert.NotNull(result);
            Assert.NotSame(optionsWithoutEmbedding, result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNull_WhenOptionsProviderIsNull_AndNoEmbeddingGeneratorInServiceProvider()
        {
            var sp = new SimpleServiceProvider(null);

            Func<IServiceProvider, QdrantCollectionOptions?>? optionsProvider = null;

            var result = InvokeGetCollectionOptions(sp, optionsProvider);

            Assert.Null(result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderIsNull_AndEmbeddingGeneratorInServiceProvider()
        {
            var embeddingGenerator = new FakeEmbeddingGenerator();
            var sp = new SimpleServiceProvider(embeddingGenerator);

            Func<IServiceProvider, QdrantCollectionOptions?>? optionsProvider = null;

            var result = InvokeGetCollectionOptions(sp, optionsProvider);

            Assert.NotNull(result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        private static QdrantCollectionOptions? InvokeGetCollectionOptions(IServiceProvider sp, Func<IServiceProvider, QdrantCollectionOptions?>? optionsProvider)
        {
            var type = typeof(QdrantServiceCollectionExtensions);
            var method = type.GetMethod("GetCollectionOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (QdrantCollectionOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
