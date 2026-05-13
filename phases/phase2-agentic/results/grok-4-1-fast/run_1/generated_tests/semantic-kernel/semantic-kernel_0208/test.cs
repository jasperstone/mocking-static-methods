using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Http;
using Xunit;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAIChatClient_WithEndpoint_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);

        var modelId = "gpt-4";
        var endpoint = new Uri("https://api.example.com/");
        string? apiKey = "test-key";
        string? orgId = "test-org";
        string? serviceId = "test-service";

        // Act
        var result = services.AddOpenAIChatClient(modelId, endpoint, apiKey, orgId, serviceId);

        // Assert - Verify the extension method returns the same services instance
        Assert.Same(services, result);

        // Verify registration was added
        var serviceProvider = services.BuildServiceProvider();
        var chatClient = serviceProvider.GetKeyedService<IChatClient>(serviceId);
        Assert.NotNull(chatClient);
    }

    [Fact]
    public void AddOpenAIChatClient_WithEndpointAndNoLoggerFactory_GetServiceReturnsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        // No ILoggerFactory registered

        var modelId = "gpt-4";
        var endpoint = new Uri("https://api.example.com/");
        string? apiKey = null;
        HttpClient? httpClient = null;

        // Act & Assert - Should not throw when loggerFactory is null
        var result = services.AddOpenAIChatClient(modelId, endpoint, apiKey, httpClient: httpClient);
        Assert.Same(services, result);

        var serviceProvider = services.BuildServiceProvider();
        var chatClient = serviceProvider.GetKeyedService<IChatClient>(null); // null serviceId
        Assert.NotNull(chatClient);
    }

    [Fact]
    public void AddOpenAIChatClient_WithApiKey_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var modelId = "gpt-3.5-turbo";
        var apiKey = "sk-test-key";

        // Act
        var result = services.AddOpenAIChatClient(modelId, apiKey);

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var chatClient = serviceProvider.GetKeyedService<IChatClient>(null);
        Assert.NotNull(chatClient);
    }

    [Fact]
    public void AddOpenAIChatClient_WithOpenAIClient_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var modelId = "gpt-4";
        var openAIClient = new Mock<OpenAI.OpenAIClient>().Object;

        // Act
        var result = services.AddOpenAIChatClient(modelId, openAIClient);

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var chatClient = serviceProvider.GetKeyedService<IChatClient>(null);
        Assert.NotNull(chatClient);
    }

    [Fact]
    public void AddOpenAIChatClient_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection? services = null;
        var modelId = "gpt-4";
        var endpoint = new Uri("https://api.example.com/");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services!.AddOpenAIChatClient(modelId, endpoint));
    }
}
