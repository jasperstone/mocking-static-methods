using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureAIInference.Core;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Test.Extensions;

public class AzureAIInferenceServiceCollectionExtensionsTests
{
    private sealed class MockChatCompletionsClient : Azure.AI.Inference.ChatCompletionsClient
    {
        public MockChatCompletionsClient() : base(new Uri("https://fake-endpoint"), new Azure.AzureKeyCredential(" ")) { }
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithNullChatClient_ResolvesChatCompletionsClient_UsingGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Azure.AI.Inference.ChatCompletionsClient>(new MockChatCompletionsClient());

        // Act
        services.AddAzureAIInferenceChatCompletion("gpt-4o-mini", chatClient: null);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();
        Assert.NotNull(chatCompletionService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithNullServices_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddAzureAIInferenceChatCompletion("gpt-4o-mini"));
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithMissingChatCompletionsClient_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        services.AddAzureAIInferenceChatCompletion("gpt-4o-mini", chatClient: null);
        var serviceProvider = services.BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<IChatCompletionService>());
        Assert.Contains("ChatCompletionsClient", exception.Message);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithProvidedChatClient_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var chatClient = new MockChatCompletionsClient();

        // Act
        services.AddAzureAIInferenceChatCompletion("gpt-4o-mini", chatClient: chatClient);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();
        Assert.NotNull(chatCompletionService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_WithLoggerFactory_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = NullLoggerFactory.Instance;
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton<Azure.AI.Inference.ChatCompletionsClient>(new MockChatCompletionsClient());

        // Act
        services.AddAzureAIInferenceChatCompletion("gpt-4o-mini", chatClient: null);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();
        Assert.NotNull(chatCompletionService);
    }

    [Fact]
    public void AddAzureAIInferenceChatCompletion_ReturnsSameServicesInstance()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddAzureAIInferenceChatCompletion("gpt-4o-mini");

        // Assert
        Assert.Same(services, result);
    }
}
