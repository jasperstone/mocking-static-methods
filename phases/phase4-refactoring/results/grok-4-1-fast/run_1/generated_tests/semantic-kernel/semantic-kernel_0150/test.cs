using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Ollama.UnitTests.Extensions;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaChatCompletion_WithEndpoint_RegistersServiceThatExecutesGetServiceCall()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton<ILoggerFactory>(mockLoggerFactory);

        // Act
        services.AddOllamaChatCompletion("llama3", new Uri("http://localhost:11434"));

        // Assert - Force resolution to trigger the factory delegate containing GetService call
        using var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
        Assert.Equal(1, mockLoggerFactory.CreateLoggerInvocationCount);
    }

    [Fact]
    public void AddOllamaChatCompletion_WithHttpClient_RegistersServiceThatExecutesGetServiceCall()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton<ILoggerFactory>(mockLoggerFactory);

        // Act
        services.AddOllamaChatCompletion("llama3", httpClient: new DummyHttpClient());

        // Assert - Force resolution to trigger the factory delegate containing GetService call
        using var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
        Assert.Equal(1, mockLoggerFactory.CreateLoggerInvocationCount);
    }

    [Fact]
    public void AddOllamaChatCompletion_NoLoggerFactoryAvailable_StillRegistersService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOllamaChatCompletion("llama3", new Uri("http://localhost:11434"));

        // Assert - Service resolves successfully even when GetService returns null
        using var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
    }

    private class MockLoggerFactory : ILoggerFactory
    {
        public int CreateLoggerInvocationCount { get; private set; }

        public void Dispose() { }

        public ILogger CreateLogger(string categoryName)
        {
            CreateLoggerInvocationCount++;
            return NullLogger.Instance;
        }

        public void AddProvider(ILoggerProvider provider) { }
    }

    private class DummyHttpClient : HttpClient { }
}
