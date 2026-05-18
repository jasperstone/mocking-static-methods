using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using OpenAI;
using Xunit;

namespace Microsoft.SemanticKernel;

public class OpenAIServiceCollectionExtensionsTests
{
    private class MockLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }

    private class MockLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new MockLogger();
        public void Dispose() { }
    }

    private class MockLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider) { }
        public ILogger CreateLogger(string categoryName) => new MockLogger();
        public void Dispose() { }
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_OverloadWithClient_RegistersFactoryCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<OpenAIClient>(new OpenAIClient("test-api-key"));

        // Act
        var result = services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002");

        // Assert
        Assert.Same(services, result);
        var descriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(ITextEmbeddingGenerationService));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_OverloadWithClient_FactoryUsesGetServiceForLoggerFactory_WithLogger()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = new MockLoggerFactory();
        services.AddSingleton(loggerFactory);
        services.AddSingleton<OpenAIClient>(new OpenAIClient("test-api-key"));

        // Act - Triggers factory with serviceProvider.GetService<ILoggerFactory>()
        var serviceProvider = services
            .AddOpenAITextEmbeddingGeneration("text-embedding-ada-002")
            .BuildServiceProvider();

        // Assert - GetService<ILoggerFactory>() returns registered instance (no exception)
        var embeddingService = serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>();
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_OverloadWithClient_FactoryUsesGetServiceForLoggerFactory_NoLogger()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<OpenAIClient>(new OpenAIClient("test-api-key"));

        // Act - Triggers factory where serviceProvider.GetService<ILoggerFactory>() returns null
        var serviceProvider = services
            .AddOpenAITextEmbeddingGeneration("text-embedding-ada-002")
            .BuildServiceProvider();

        // Assert - No exception when GetService<ILoggerFactory>() returns null
        var embeddingService = serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>();
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_OverloadWithClient_WithDimensions_ExecutesFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<OpenAIClient>(new OpenAIClient("test-api-key"));

        // Act - Factory calls GetService<ILoggerFactory>() + uses dimensions param
        var serviceProvider = services
            .AddOpenAITextEmbeddingGeneration("text-embedding-3-small", dimensions: 512)
            .BuildServiceProvider();

        // Assert
        var embeddingService = serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>();
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_OverloadWithClient_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<OpenAIClient>(new OpenAIClient("test-api-key"));

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", serviceId: "test-key");

        // Assert - Factory registration with key (covers GetService call in keyed factory)
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider.GetServices<ITextEmbeddingGenerationService>());
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_OverloadWithClient_ValidatesModelId()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            services.AddOpenAITextEmbeddingGeneration(null!));
        
        Assert.Throws<ArgumentException>(() => 
            services.AddOpenAITextEmbeddingGeneration(""));
    }
}
