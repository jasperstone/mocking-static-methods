using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureAIInference.Core;
using Xunit;
using Azure.AI.Inference;

namespace Microsoft.SemanticKernel;

public class AzureAIInferenceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureAIInferenceChatCompletion_ThrowsInvalidOperationException_WhenChatClientNotInServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

        // Act
        services.AddAzureAIInferenceChatCompletion("model-id", chatClient: null);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetRequiredKeyedService<IChatCompletionService>(null));
        Assert.Contains("ChatCompletionsClient", exception.Message);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithChatClientInServiceProvider_Succeeds()
    {
        // Arrange
        var mockChatClient = new MockChatCompletionsClient();
        var services = new ServiceCollection();
        services.AddSingleton(mockChatClient);
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

        // Act
        services.AddAzureAIInferenceChatCompletion("model-id", chatClient: null);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var chatService = serviceProvider.GetRequiredKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithCustomServiceId_RegistersWithCorrectKey()
    {
        // Arrange
        var mockChatClient = new MockChatCompletionsClient();
        var services = new ServiceCollection();
        services.AddSingleton(mockChatClient);
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

        const string serviceId = "test-service";

        // Act
        services.AddAzureAIInferenceChatCompletion("model-id", chatClient: null, serviceId: serviceId);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var chatService = serviceProvider.GetRequiredKeyedService<IChatCompletionService>(serviceId);
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithExplicitChatClient_DoesNotUseServiceProvider()
    {
        // Arrange
        var explicitChatClient = new MockChatCompletionsClient();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

        // Act
        services.AddAzureAIInferenceChatCompletion("model-id", chatClient: explicitChatClient);
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Should succeed without ChatCompletionsClient in DI
        var chatService = serviceProvider.GetRequiredKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
    }

    private class MockChatCompletionsClient : ChatCompletionsClient
    {
        public MockChatCompletionsClient() : base(new Uri("http://test"), new MockKeyCredential()) { }
    }

    private class MockKeyCredential : Azure.Core.AzureKeyCredential
    {
        public MockKeyCredential() : base("test") { }
    }
}
