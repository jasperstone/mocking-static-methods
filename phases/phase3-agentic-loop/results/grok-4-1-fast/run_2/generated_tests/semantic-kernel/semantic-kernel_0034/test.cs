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
    public void AddAzureAIInferenceChatCompletion_WithChatClientInProvider_SuccessfullyResolvesChatService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ChatCompletionsClient>(new MockChatCompletionsClient());

        // Act
        services.AddAzureAIInferenceChatCompletion("gpt-4o-mini");

        var serviceProvider = services.BuildServiceProvider();

        // Assert - should not throw when GetRequiredService is called internally
        var chatService = serviceProvider.GetRequiredKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_NoChatClientInProvider_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        services.AddAzureAIInferenceChatCompletion("gpt-4o-mini");
        var serviceProvider = services.BuildServiceProvider();

        Assert.ThrowsAny<InvalidOperationException>(() => 
            serviceProvider.GetRequiredKeyedService<IChatCompletionService>(null));
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithProvidedChatClient_SuccessfullyResolvesChatService()
    {
        // Arrange
        var services = new ServiceCollection();
        var providedChatClient = new MockChatCompletionsClient();

        // Act
        services.AddAzureAIInferenceChatCompletion("gpt-4o-mini", providedChatClient);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var chatService = serviceProvider.GetRequiredKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithLoggerFactory_SuccessfullyResolvesChatService()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = new LoggerFactory();
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton(new MockChatCompletionsClient());

        // Act
        services.AddAzureAIInferenceChatCompletion("gpt-4o-mini");

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var chatService = serviceProvider.GetRequiredKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
    }
}

// Simple mock that uses the base constructor without credentials
public class MockChatCompletionsClient : ChatCompletionsClient
{
    public MockChatCompletionsClient() : base("https://mock-endpoint.com", new MockKeyCredential()) { }
}

public class MockKeyCredential : Azure.Core.TokenCredential
{
    public override AccessToken GetToken(Azure.Core.TokenRequestContext requestContext, System.Threading.CancellationToken cancellationToken)
        => new("mock-token", DateTimeOffset.UtcNow.AddHours(1));
}
