using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSharp;
using Xunit;

namespace SemanticKernel.Connectors.Ollama.Tests
{
    public sealed class OllamaServiceCollectionExtensionsTests
    {
        private sealed class TestOllamaClient : OllamaApiClient
        {
            private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;

            public bool BuildCalled { get; private set; }
            public IServiceProvider? CapturedServiceProvider { get; private set; }

            public TestOllamaClient(IEmbeddingGenerator<string, Embedding<float>> generator)
                : base(new Uri("http://localhost:11434"))
            {
                _generator = generator;
            }

            public override IEmbeddingGenerator<string, Embedding<float>> AsBuilder() =>
                new DelegatingEmbeddingGenerator(
                    build: sp =>
                    {
                        BuildCalled = true;
                        CapturedServiceProvider = sp;
                        return _generator;
                    });
        }

        private sealed class DelegatingEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
        {
            private readonly Func<IServiceProvider, IEmbeddingGenerator<string, Embedding<float>>> _build;

            public DelegatingEmbeddingGenerator(Func<IServiceProvider, IEmbeddingGenerator<string, Embedding<float>>> build)
            {
                _build = build;
            }

            public IEmbeddingGenerator<string, Embedding<float>> Build(IServiceProvider services) => _build(services);

            public IEmbeddingGenerationService<string, Embedding<float>> AsTextEmbeddingGenerationService(IServiceProvider services)
                => new DelegatingEmbeddingGenerationService();
        }

        private sealed class DelegatingEmbeddingGenerationService : IEmbeddingGenerationService<string, Embedding<float>>
        {
            public Task<IReadOnlyList<Embedding<float>>> GenerateEmbeddingsAsync(string data, CancellationToken cancellationToken = default)
                => Task.FromResult<IReadOnlyList<Embedding<float>>>(Array.Empty<Embedding<float>>());

            public Task<IReadOnlyList<Embedding<float>>> GenerateEmbeddingsAsync(IList<string> data, CancellationToken cancellationToken = default)
                => Task.FromResult<IReadOnlyList<Embedding<float>>>(Array.Empty<Embedding<float>>());
        }

        [Fact]
        public void AddOllamaTextEmbeddingGeneration_UsesServiceProviderGetServiceFallback()
        {
            // Arrange
            var services = new ServiceCollection();
            var embeddingGenerator = new DelegatingEmbeddingGenerator(_ => new DelegatingEmbeddingGenerationService());
            var client = new TestOllamaClient(embeddingGenerator);

            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(client);

            // Act
            services.AddOllamaTextEmbeddingGeneration();
            var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredService<ITextEmbeddingGenerationService>();

            // Assert
            Assert.NotNull(service);
            Assert.True(client.BuildCalled);
            Assert.Same(provider, client.CapturedServiceProvider);
        }
    }
}
