using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
using Xunit;

namespace SemanticKernel.Connectors.HuggingFace.UnitTests
{
    public class HuggingFaceServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_WithLoggerFactory_UsesLoggerFactory()
        {
            var services = new ServiceCollection();
            var loggerFactory = new TrackingLoggerFactory();
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            using var httpClient = new HttpClient();

            services.AddHuggingFaceTextEmbeddingGeneration(
                endpoint: new Uri("https://example.com"),
                apiKey: "test-api-key",
                serviceId: "service",
                httpClient: httpClient);

            using var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredKeyedService<ITextEmbeddingGenerationService>("service");

            Assert.IsType<HuggingFaceTextEmbeddingGenerationService>(service);
            Assert.Equal(typeof(HuggingFaceTextEmbeddingGenerationService).FullName, loggerFactory.LastCategoryName);
        }

        [Fact]
        public void AddHuggingFaceTextEmbeddingGeneration_WithoutLoggerFactory_Succeeds()
        {
            var services = new ServiceCollection();
            using var httpClient = new HttpClient();

            services.AddHuggingFaceTextEmbeddingGeneration(
                endpoint: new Uri("https://example.com"),
                serviceId: "service",
                httpClient: httpClient);

            using var provider = services.BuildServiceProvider();
            var service = provider.GetRequiredKeyedService<ITextEmbeddingGenerationService>("service");

            Assert.IsType<HuggingFaceTextEmbeddingGenerationService>(service);
        }

        private sealed class TrackingLoggerFactory : ILoggerFactory
        {
            public string? LastCategoryName { get; private set; }

            public void AddProvider(ILoggerProvider provider)
            {
            }

            public ILogger CreateLogger(string categoryName)
            {
                this.LastCategoryName = categoryName;
                return new NoOpLogger();
            }

            public void Dispose()
            {
            }

            private sealed class NoOpLogger : ILogger
            {
                public IDisposable? BeginScope<TState>(TState state) => NullScope.Instance;

                public bool IsEnabled(LogLevel logLevel) => false;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                {
                }

                private sealed class NullScope : IDisposable
                {
                    public static NullScope Instance { get; } = new NullScope();

                    public void Dispose()
                    {
                    }
                }
            }
        }
    }
}
