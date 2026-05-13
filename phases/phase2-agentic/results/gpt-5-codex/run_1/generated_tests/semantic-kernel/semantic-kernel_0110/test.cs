using System;
using System.Net.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class VertexAIServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddVertexAIEmbeddingGenerator_ResolvesLoggerFactoryFromServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            bool loggerFactoryResolved = false;

            services.AddSingleton<ILoggerFactory>(_ =>
            {
                loggerFactoryResolved = true;
                return new NoopLoggerFactory();
            });

            using var httpClient = new HttpClient();

            services.AddVertexAIEmbeddingGenerator(
                modelId: "test-model",
                bearerKey: "test-bearer-key",
                location: "us-central1",
                projectId: "test-project",
                serviceId: "test-service",
                httpClient: httpClient);

            using var provider = services.BuildServiceProvider();

            // Act
            var generator = provider.GetRequiredKeyedService<IEmbeddingGenerator<string, Embedding<float>>>("test-service");

            // Assert
            Assert.NotNull(generator);
            Assert.True(loggerFactoryResolved);
        }

        private sealed class NoopLoggerFactory : ILoggerFactory
        {
            public ILogger CreateLogger(string categoryName) => NoopLogger.Instance;

            public void AddProvider(ILoggerProvider provider)
            {
            }

            public void Dispose()
            {
            }

            private sealed class NoopLogger : ILogger
            {
                public static readonly ILogger Instance = new NoopLogger();

                public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;

                public bool IsEnabled(LogLevel logLevel) => false;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                {
                }
            }

            private sealed class NoopDisposable : IDisposable
            {
                public static readonly IDisposable Instance = new NoopDisposable();

                public void Dispose()
                {
                }
            }
        }
    }
}
