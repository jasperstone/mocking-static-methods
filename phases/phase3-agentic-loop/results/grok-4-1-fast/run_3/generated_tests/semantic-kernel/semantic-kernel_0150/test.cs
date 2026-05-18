using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaChatCompletion_WithEndpoint_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        string modelId = "test-model";
        var endpoint = new Uri("http://localhost:11434");

        // Act
        var result = services.AddOllamaChatCompletion(modelId, endpoint);

        // Assert
        Assert.Same(services, result);
        Assert.Single(services);

        var descriptor = Assert.Single(services);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Equal(typeof(IChatCompletionService), descriptor.ServiceType);

        // Build provider - forces factory execution which calls serviceProvider.GetService<ILoggerFactory>()
        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetRequiredService<IChatCompletionService>();

        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddOllamaChatCompletion_WithHttpClient_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        string modelId = "test-model";
        using var httpClient = new HttpClient();

        // Act
        var result = services.AddOllamaChatCompletion(modelId, httpClient);

        // Assert
        Assert.Same(services, result);
        Assert.Single(services);

        var descriptor = Assert.Single(services);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Equal(typeof(IChatCompletionService), descriptor.ServiceType);

        // Build provider - forces factory execution which calls serviceProvider.GetService<ILoggerFactory>()
        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetRequiredService<IChatCompletionService>();

        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddOllamaTextGeneration_WithClient_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var mockClient = new Mock<OllamaSharp.OllamaApiClient>(new Uri("http://localhost:11434"), "test-model").Object;

        // Act
        var result = services.AddOllamaTextGeneration("test-model", mockClient);

        // Assert
        Assert.Same(services, result);
        Assert.Single(services);

        var descriptor = Assert.Single(services);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Equal(typeof(ITextGenerationService), descriptor.ServiceType);

        // Build provider - forces factory execution which calls serviceProvider.GetService<ILoggerFactory>()
        var serviceProvider = services.BuildServiceProvider();
        var textGenService = serviceProvider.GetRequiredService<ITextGenerationService>();

        Assert.NotNull(textGenService);
    }

    [Fact]
    public void AddOllamaTextGeneration_AutoClientDiscovery_CallsMultipleGetServiceMethods()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(new Mock<OllamaSharp.OllamaApiClient>(new Uri("http://localhost:11434"), "test-model").Object);
        services.AddLogging();

        // Act
        var result = services.AddOllamaTextGeneration(ollamaClient: null);

        // Assert
        Assert.Same(services, result);

        // Build provider - forces factory execution which calls multiple serviceProvider.Get* methods
        var serviceProvider = services.BuildServiceProvider();
        var textGenService = serviceProvider.GetRequiredService<ITextGenerationService>();

        Assert.NotNull(textGenService);
    }
}
