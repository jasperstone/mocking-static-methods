using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Xunit;
using Azure.AI.Inference;

namespace Microsoft.SemanticKernel;

public class AzureAIInferenceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureAIInferenceChatCompletion_ThrowsInvalidOperationException_WhenChatClientNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddAzureAIInferenceChatCompletion("model-id", chatClient: null);

        var provider = services.BuildServiceProvider();

        // Assert - GetRequiredService throws InvalidOperationException when ChatCompletionsClient not registered
        Assert.ThrowsAny<InvalidOperationException>(() => provider.GetKeyedService<IChatCompletionService>(null));
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithChatClientRegistered_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockChatClient = new ChatCompletionsClient(new Uri("https://example.com"), new Azure.Core.AzureKeyCredential("test"));
        services.AddSingleton(mockChatClient);

        // Act
        services.AddAzureAIInferenceChatCompletion("model-id", chatClient: null);

        var provider = services.BuildServiceProvider();

        // Assert
        var chatService = provider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithExplicitChatClient_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var explicitChatClient = new ChatCompletionsClient(new Uri("https://explicit.com"), new Azure.Core.AzureKeyCredential("explicit"));

        // Act
        services.AddAzureAIInferenceChatCompletion("model-id", chatClient: explicitChatClient);

        var provider = services.BuildServiceProvider();

        // Assert - Uses provided client, no GetRequiredService call needed
        var chatService = provider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockChatClient = new ChatCompletionsClient(new Uri("https://example.com"), new Azure.Core.AzureKeyCredential("test"));
        services.AddSingleton(mockChatClient);
        const string serviceId = "test-service";

        // Act
        services.AddAzureAIInferenceChatCompletion("model-id", chatClient: null, serviceId: serviceId);

        var provider = services.BuildServiceProvider();

        // Assert
        var chatService = provider.GetKeyedService<IChatCompletionService>(serviceId);
        Assert.NotNull(chatService);
        var nullService = provider.GetKeyedService<IChatCompletionService>(null);
        Assert.Null(nullService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithLogger_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var mockChatClient = new ChatCompletionsClient(new Uri("https://example.com"), new Azure.Core.AzureKeyCredential("test"));
        services.AddSingleton(mockChatClient);
        services.AddSingleton<ILoggerFactory>(loggerFactory);

        // Act
        services.AddAzureAIInferenceChatCompletion("model-id", chatClient: null);

        var provider = services.BuildServiceProvider();

        // Assert
        var chatService = provider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
    }
}
