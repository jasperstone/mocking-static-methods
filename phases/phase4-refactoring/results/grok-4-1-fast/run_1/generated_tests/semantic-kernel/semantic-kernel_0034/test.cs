using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Xunit;
using Azure.AI.Inference;
using Azure;

namespace Microsoft.SemanticKernel;

public class AzureAIInferenceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAzureAIInferenceChatCompletion_ThrowsInvalidOperationException_WhenChatClientNullAndNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Registration succeeds, but resolution throws due to GetRequiredService
        services.AddAzureAIInferenceChatCompletion("model-id", chatClient: null);
        var serviceProvider = services.BuildServiceProvider();
        
        var exception = Assert.Throws<InvalidOperationException>(() => 
            serviceProvider.GetRequiredService<IChatCompletionService>());
        Assert.Contains("ChatCompletionsClient", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_Succeeds_WhenChatClientNullButRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ChatCompletionsClient>(new MockChatCompletionsClient());

        // Act - Triggers GetRequiredService on line 133
        services.AddAzureAIInferenceChatCompletion("model-id", chatClient: null);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetRequiredService<IChatCompletionService>();
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_Succeeds_WhenChatClientProvided()
    {
        // Arrange
        var expectedChatClient = new MockChatCompletionsClient();
        var services = new ServiceCollection();

        // Act - Skips GetRequiredService due to non-null chatClient
        services.AddAzureAIInferenceChatCompletion("model-id", expectedChatClient);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetRequiredService<IChatCompletionService>();
        Assert.NotNull(chatService);
    }

    private class MockChatCompletionsClient : ChatCompletionsClient
    {
        public MockChatCompletionsClient() : base(new Uri("http://test"), new AzureKeyCredential("test")) { }
    }
}
