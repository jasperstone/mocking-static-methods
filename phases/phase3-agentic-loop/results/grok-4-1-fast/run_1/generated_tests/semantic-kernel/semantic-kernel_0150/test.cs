using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaChatCompletion_WithEndpoint_AddsCorrectRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>().Object;
        services.AddSingleton(mockLoggerFactory);

        var modelId = "test-model";
        var endpoint = new Uri("http://localhost:11434");

        // Act
        var result = services.AddOllamaChatCompletion(modelId, endpoint);

        // Assert registration was added (ignore the logger we added)
        Assert.NotNull(result);
        var newRegistrations = result.Where(d => d.ServiceType == typeof(IChatCompletionService)).ToList();
        Assert.Single(newRegistrations);

        // Verify the factory creates the service (exercises GetService<ILoggerFactory>())
        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetRequiredService<IChatCompletionService>();
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddOllamaChatCompletion_WithServiceId_AddsKeyedRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>().Object;
        services.AddSingleton(mockLoggerFactory);

        var modelId = "test-model";
        var endpoint = new Uri("http://localhost:11434");
        var serviceId = "test-service";

        // Act
        var result = services.AddOllamaChatCompletion(modelId, endpoint, serviceId);

        // Assert registration was added (ignore the logger we added)
        Assert.NotNull(result);
        var newRegistrations = result.Where(d => d.ServiceType == typeof(IChatCompletionService)).ToList();
        Assert.Single(newRegistrations);

        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(serviceId);
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddOllamaChatCompletion_WithoutLoggerFactory_HandlesNullGracefully()
    {
        // Arrange
        var services = new ServiceCollection(); // No ILoggerFactory registered

        var modelId = "test-model";
        var endpoint = new Uri("http://localhost:11434");

        // Act & Assert
        var result = services.AddOllamaChatCompletion(modelId, endpoint);
        Assert.NotNull(result);

        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetRequiredService<IChatCompletionService>();
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddOllamaChatCompletion_WithHttpClient_AddsCorrectRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>().Object;
        services.AddSingleton(mockLoggerFactory);

        var modelId = "test-model";
        var httpClient = new Mock<HttpClient>().Object;

        // Act
        var result = services.AddOllamaChatCompletion(modelId, httpClient);

        // Assert registration was added (ignore the logger we added)
        Assert.NotNull(result);
        var newRegistrations = result.Where(d => d.ServiceType == typeof(IChatCompletionService)).ToList();
        Assert.Single(newRegistrations);

        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetRequiredService<IChatCompletionService>();
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddOllamaTextGeneration_WithEndpoint_CallsGetService()
    {
        // Test the other GetService call pattern
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>().Object;
        services.AddSingleton(mockLoggerFactory);

        var modelId = "test-model";
        var endpoint = new Uri("http://localhost:11434");

        var result = services.AddOllamaTextGeneration(modelId, endpoint);

        Assert.NotNull(result);
        var newRegistrations = result.Where(d => d.ServiceType == typeof(ITextGenerationService)).ToList();
        Assert.Single(newRegistrations);

        var serviceProvider = services.BuildServiceProvider();
        var textService = serviceProvider.GetRequiredService<ITextGenerationService>();
        Assert.NotNull(textService);
    }
}
