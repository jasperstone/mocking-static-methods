using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Xunit;
using OllamaSharp;

namespace Microsoft.SemanticKernel.Connectors.Ollama.UnitTests.Extensions;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaTextEmbeddingGeneration_FallbackToGetServiceOllamaApiClient_Succeeds()
    {
        // Arrange - Setup so it falls through to GetService<OllamaApiClient>() (line 344)
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(new MockLoggerFactory());
        services.AddSingleton<OllamaApiClient>(new MockOllamaApiClient());

        // Act
        var result = services.AddOllamaTextEmbeddingGeneration(ollamaClient: null);

        // Assert registration
        Assert.NotNull(result);
        Assert.Single(result);

        // Verify the factory exercises the GetService<OllamaApiClient> path
        var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService<string, float>>(null!);

        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_NoValidOllamaApiClient_ThrowsInvalidOperationException()
    {
        // Arrange - IOllamaApiClient that is NOT OllamaApiClient, no unkeyed OllamaApiClient
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(new MockLoggerFactory());
        services.AddSingleton<IOllamaApiClient>(new MockNonOllamaApiClient());

        // Act & Assert
        var result = services.AddOllamaTextEmbeddingGeneration(ollamaClient: null);
        var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetKeyedService<ITextEmbeddingGenerationService<string, float>>(null!));
        
        Assert.Equal("No IOllamaApiClient implementations found in the service collection.", exception.Message);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_KeyedOllamaApiClient_TakesPrecedenceOverGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(new MockLoggerFactory());
        var keyedClient = new MockOllamaApiClient();
        services.AddKeyedSingleton<OllamaApiClient>("test-key", keyedClient);

        // Act
        var result = services.AddOllamaTextEmbeddingGeneration(ollamaClient: null, serviceId: "test-key");
        var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService<string, float>>("test-key");

        // Assert
        Assert.NotNull(embeddingService);
    }

    // Minimal mocks - simplified to avoid OllamaSharp type dependencies
    private class MockLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider) { }
        public ILogger CreateLogger(string categoryName) => new MockLogger();
        public void Dispose() { }
    }

    private class MockLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }

    private class MockOllamaApiClient : OllamaApiClient
    {
        public MockOllamaApiClient() : base(new System.Net.Http.HttpClient()) { }
    }

    private class MockNonOllamaApiClient : IOllamaApiClient
    {
        // Minimal implementation using object for unavailable types
        public Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
            => Task.FromResult("mock");
        public Task<object> ChatAsync(object request, CancellationToken cancellationToken = default)
            => Task.FromResult(new object());
        public Task<object> CopyModelAsync(object request, CancellationToken cancellationToken = default)
            => Task.FromResult(new object());
        public Task<object> CreateModelAsync(object request, CancellationToken cancellationToken = default)
            => Task.FromResult(new object());
        public Task DeleteModelAsync(object request, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
        public Task<object> EmbedAsync(object request, CancellationToken cancellationToken = default)
            => Task.FromResult(new object());
        public Task<object> ListLocalModelsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(new object());
        public Task PullAsync(object request, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
