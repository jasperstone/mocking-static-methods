using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Ollama.UnitTests.Extensions;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaChatCompletion_WithEndpoint_AddsKeyedSingletonService()
    {
        // Arrange
        var services = new ServiceCollection();
        var modelId = "test-model";
        var endpoint = new Uri("http://localhost:11434");

        // Act
        var result = services.AddOllamaChatCompletion(modelId, endpoint);

        // Assert
        Assert.Same(services, result);
        Assert.Single(services);

        var descriptor = Assert.Single(services);
        Assert.Equal(typeof(IChatCompletionService), descriptor.ServiceType);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Null(descriptor.Key);
    }

    [Fact]
    public void AddOllamaChatCompletion_WithEndpoint_FactoryExecutesGetServiceCall()
    {
        // Arrange
        var services = new ServiceCollection();
        var modelId = "test-model";
        var endpoint = new Uri("http://localhost:11434");
        services.AddOllamaChatCompletion(modelId, endpoint);

        var descriptor = Assert.Single(services);
        var factory = (Func<IServiceProvider, object?>)descriptor.ImplementationFactory!;

        // Act & Assert - Factory executes without ILoggerFactory (GetService returns null) and succeeds
        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var chatService = factory(serviceProvider);
        Assert.NotNull(chatService);
        Assert.IsAssignableFrom<IChatCompletionService>(chatService);
    }

    [Fact]
    public void AddOllamaChatCompletion_WithHttpClient_AddsKeyedSingletonService()
    {
        // Arrange
        var services = new ServiceCollection();
        var modelId = "test-model";
        using var httpClient = new HttpClient();

        // Act
        var result = services.AddOllamaChatCompletion(modelId, httpClient);

        // Assert
        Assert.Same(services, result);
        Assert.Single(services);

        var descriptor = Assert.Single(services);
        Assert.Equal(typeof(IChatCompletionService), descriptor.ServiceType);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddOllamaChatCompletion_WithHttpClient_FactoryExecutesGetServiceCall()
    {
        // Arrange
        var services = new ServiceCollection();
        var modelId = "test-model";
        using var httpClient = new HttpClient();
        services.AddOllamaChatCompletion(modelId, httpClient);

        var descriptor = Assert.Single(services);
        var factory = (Func<IServiceProvider, object?>)descriptor.ImplementationFactory!;

        // Act & Assert - Factory calls GetService<ILoggerFactory>() (returns null) but succeeds
        using var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var chatService = factory(serviceProvider);
        Assert.NotNull(chatService);
        Assert.IsAssignableFrom<IChatCompletionService>(chatService);
    }

    [Fact]
    public void AddOllamaChatCompletion_WithILoggerFactoryAvailable_FactoryExecutesSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        var modelId = "test-model";
        var endpoint = new Uri("http://localhost:11434");
        services.AddOllamaChatCompletion(modelId, endpoint);

        var descriptor = Assert.Single(services.Where(d => d.ServiceType == typeof(IChatCompletionService)));
        var factory = (Func<IServiceProvider, object?>)descriptor.ImplementationFactory!;

        // Act
        using var serviceProvider = services.BuildServiceProvider();
        var chatService = factory(serviceProvider);

        // Assert - Verifies GetService<ILoggerFactory>() succeeds when service is available
        Assert.NotNull(chatService);
        Assert.IsAssignableFrom<IChatCompletionService>(chatService);
    }
}
